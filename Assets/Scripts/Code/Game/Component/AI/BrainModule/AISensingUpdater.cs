using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 感知模块
    /// </summary>
    public class AISensingUpdater : BrainModuleBase
    {
        private AIManager aiManager;
        //确认感知的敌人列表
        private DictionaryComponent<long, SensibleInfo> enemySensibles;
        //预处理敌人列表
        private DictionaryComponent<long, SensibleInfo> enemySensiblesPreparation;

        private AISensingKnowledge sensingKnowledge;

        protected override void InitInternal()
        {
            base.InitInternal();
            aiManager = knowledge.AIManager;
            sensingKnowledge = knowledge.SensingKnowledge;
            enemySensibles = DictionaryComponent<long, SensibleInfo>.Create();
            enemySensiblesPreparation = DictionaryComponent<long, SensibleInfo>.Create();
            sensingKnowledge.EnemySensibles = enemySensibles;
        }
        

        protected override void ClearInternal()
        {
            base.ClearInternal();
            sensingKnowledge.EnemySensibles = null;
            sensingKnowledge = null;
            aiManager = null;
            enemySensibles.Dispose();
            enemySensibles = null;
            enemySensiblesPreparation.Dispose();
            enemySensiblesPreparation = null;
        }

        protected override void UpdateMainThreadInternal()
        {
            base.UpdateMainThreadInternal();
            knowledge.DefendAreaKnowledge.IsInDefendRange =
                knowledge.DefendAreaKnowledge.CheckInDefendArea(knowledge.Entity.Position);
            if (knowledge.EyeTransform != null)
            {
                knowledge.EyePos = knowledge.EyeTransform.position;
            }
            else if (knowledge.Entity?.ConfigActor?.Common != null)
            {
                knowledge.EyePos = knowledge.CurrentPos + Vector3.up * knowledge.Entity.ConfigActor.Common.ModelHeight;
            }
            else
            {
                knowledge.EyePos = knowledge.CurrentPos;
            }
            CollectEnemies();
            ProcessEnemies();
        }

        private void CollectEnemies()
        {
            enemySensiblesPreparation.Clear();
            var entityList = aiManager.GetEnemies(knowledge.CampID);
            if (entityList == null) return;
            foreach (var item in entityList)
            {
                for (int i = 0; i < item.Value.Count; i++)
                {
                    var entity = item.Value[i];
                    var entityID = entity.Id;
                    var entityPos = entity.Position;
                    var selfPos = knowledge.Entity.Position;
                    var direction = (entityPos - selfPos).normalized;
                    var isActor = true;//这里必定为Actor
                    var height = entity.ConfigActor?.Common?.ModelHeight ?? 0f;
                    
                    enemySensiblesPreparation[entityID] = new SensibleInfo()
                    {
                        SensibleID = entityID,
                        Distance = Vector3.Distance(entityPos, selfPos),
                        Position = entityPos,
                        TargetablePosition = entityPos + Vector3.up * height,
                        Direction = direction,
                        IsActorEntity = isActor,
                    };

                }
            }
        }

        private void ProcessEnemies()
        {
            sensingKnowledge.NearestEnemy = 0;
            sensingKnowledge.NearestEnemyDistance = -1f;
            enemySensibles.Clear();
            foreach (var sensible in enemySensiblesPreparation)
            {
                var viewRange = sensingKnowledge.ViewRange;
                // FeelRange 是全方位近身感知，不看朝向也不需要视线
                if (sensible.Value.Distance < sensingKnowledge.FeelRange
                    || (sensible.Value.Distance < viewRange
                        && IsInViewCone(GetEyeForward(), sensible.Value.Direction,
                            sensingKnowledge.HalfHorizontalFov, sensingKnowledge.HalfVerticalFov)))
                {
                    var info = sensible.Value;
                    info.HasLineOfSight = CheckLineOfSight(info);
                    enemySensibles.TryAdd(sensible.Key, info);
                    if (sensingKnowledge.NearestEnemyDistance < 0 || info.Distance < sensingKnowledge.NearestEnemyDistance)
                    {
                        sensingKnowledge.NearestEnemyDistance = info.Distance;
                        sensingKnowledge.NearestEnemy = info.SensibleID;
                    }
                }
            }
        }

        private Vector3 GetEyeForward()
        {
            return knowledge.EyeTransform != null ? knowledge.EyeTransform.forward : knowledge.CurrentForward;
        }

        /// <summary>
        /// 目标方向是否落在视锥内。
        ///
        /// 注意这两个轴向处理是「把另一个轴对齐到 forward 后再比夹角」而不是真正的投影：
        /// 水平比较时把方向的 y 换成 forward.y，竖直比较时把 x 换成 forward.x。
        /// 这是既有感知一直在用的口径，抽成静态方法只是为了让
        /// <see cref="AIKnowledge.CanSee"/>（受击归因用）和感知筛选走同一份实现，
        /// 不要再各自复制一套（复制必然漂移：改了一边忘了另一边，AI 就会出现
        /// 「感知得到但看不见」「看得见却感知不到」这种自相矛盾）。
        /// </summary>
        public static bool IsInViewCone(Vector3 forward, Vector3 direction, float halfHorizontalFov, float halfVerticalFov)
        {
            var horizontalDirection = direction;
            horizontalDirection.y = forward.y;
            if (Vector3.Angle(forward, horizontalDirection) >= halfHorizontalFov)
                return false;

            var verticalDirection = direction;
            verticalDirection.x = forward.x;
            return Vector3.Angle(forward, verticalDirection) < halfVerticalFov;
        }

        /// <summary>
        /// 检测脚/头顶位置对眼部位置是否有视线遮挡
        /// </summary>
        private bool CheckLineOfSight(SensibleInfo info)
        {
            return CheckLineOfSight(knowledge.EyePos, info.Position, info.TargetablePosition, info.IsActorEntity);
        }

        /// <summary>
        /// 视线判定：脚和头顶各打一条线，<b>任意一条通畅就算看得见</b>。
        ///
        /// 为什么不用单点：脚部射线经常被半人高的掩体挡住，而露出的上半身明明可见；
        /// 反过来蹲在矮墙后只露头时，脚部反而更通。两点取「或」比任何单点都稳。
        /// </summary>
        public static bool CheckLineOfSight(Vector3 eyePos, Vector3 pos, Vector3 targetablePos, bool isActorEntity)
        {
            if (isActorEntity)
            {
                var hitFeet = PhysicsHelper.LinecastScene(pos, eyePos, out _);
                var hitTop = PhysicsHelper.LinecastScene(targetablePos, eyePos, out _);
                return !hitFeet || !hitTop;
            }
            return PhysicsHelper.LinecastScene(targetablePos, eyePos, out _);
        }

        /// <summary>
        /// 信号是否能被注意到
        /// </summary>
        /// <param name="knowledge"></param>
        /// <param name="checkPos"></param>
        /// <returns></returns>
        public static bool CanSignalBeNoticed(AIKnowledge knowledge, Vector3 checkPos)
        {
            var hearAttractionRange = knowledge.SensingKnowledge.HearAttractionRange;
            var selectRange = hearAttractionRange;

            var currentPos = knowledge.CurrentPos;

            float distanceToTarget = Vector3.Distance(currentPos, checkPos);

            if (distanceToTarget < selectRange)
                return true;

            return false;
        }
    }
}