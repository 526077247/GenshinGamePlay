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
            CollectEnemies();
            ProcessEnemies();
        }

        private void CollectEnemies()
        {
            enemySensiblesPreparation.Clear();
            var entityList = aiManager.GetEnemies(knowledge.CampID);
            foreach (var item in entityList)
            {
                for (int i = 0; i < item.Value.Count; i++)
                {
                    var entity = item.Value[i];
                    var entityID = entity.Id;
                    var entityPos = entity.Position;
                    var selfPos = knowledge.Entity.Position + knowledge.Entity.Rotation * knowledge.EyePos;
                    var direction = (entityPos - selfPos).normalized;
                    
                    enemySensiblesPreparation[entityID] = new SensibleInfo()
                    {
                        SensibleID = entityID,
                        Distance = Vector3.Distance(entityPos, selfPos),
                        Position = entityPos,
                        Direction = direction,
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
                var halfHorizontalFov = sensingKnowledge.ViewPanoramic ? 180f : 0.5 * sensingKnowledge.HorizontalFov;
                var halfVerticalFov = sensingKnowledge.ViewPanoramic ? 180f : 0.5 * sensingKnowledge.VerticalFov;
                
                if (sensible.Value.Distance < sensingKnowledge.FeelRange)
                {
                    enemySensibles.TryAdd(sensible.Key, sensible.Value);
                    if (sensingKnowledge.NearestEnemyDistance<0 || sensible.Value.Distance < sensingKnowledge.NearestEnemyDistance)
                    {
                        sensingKnowledge.NearestEnemyDistance = sensible.Value.Distance;
                        sensingKnowledge.NearestEnemy = sensible.Value.SensibleID;
                    }
                }
                else if (sensible.Value.Distance < viewRange)
                {
                    var forward = knowledge.Entity.Forward;
                    if (knowledge.EyeTransform != null)
                    {
                        forward = knowledge.EyeTransform.forward;
                           
                    }
                    var horizontalDirection = sensible.Value.Direction;
                    horizontalDirection.y = forward.y;
                    var horizontalAngle = Vector3.Angle(knowledge.Entity.Forward, horizontalDirection);

                    if (horizontalAngle < halfHorizontalFov)
                    {
                        var verticalDirection = sensible.Value.Direction;
                        verticalDirection.x = forward.x;
                        var verticalAngle = Vector3.Angle(knowledge.Entity.Forward, verticalDirection);
                        if (verticalAngle < halfVerticalFov)
                        {
                            enemySensibles.TryAdd(sensible.Key, sensible.Value);
                            if (sensingKnowledge.NearestEnemyDistance<0 || sensible.Value.Distance < sensingKnowledge.NearestEnemyDistance)
                            {
                                sensingKnowledge.NearestEnemyDistance = sensible.Value.Distance;
                                sensingKnowledge.NearestEnemy = sensible.Value.SensibleID;
                            }
                        }
                    }
                }
            }
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