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
            this.aiManager = knowledge.AiManager;
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
                knowledge.DefendAreaKnowledge.CheckInDefendArea(knowledge.AiOwnerEntity.Position);
            CollectEnemies();
            ProcessEnemies();
        }

        private void CollectEnemies()
        {
            var entityList = aiManager.GetEnemies(knowledge.CampID);
            foreach (var item in entityList)
            {
                foreach (var entity in item.Value)
                {
                    var entityID = entity.Id;
                    var entityPos = entity.Position;
                    var selfPos = knowledge.AiOwnerEntity.Position + knowledge.AiOwnerEntity.Rotation * knowledge.EyePos;
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
                var viewRange = sensingKnowledge.Setting.ViewRange;
                var halfHorizontalFov = sensingKnowledge.Setting.ViewPanoramic ? 180f : 0.5 * sensingKnowledge.Setting.HorizontalFov;
                var halfVerticalFov = sensingKnowledge.Setting.ViewPanoramic ? 180f : 0.5 * sensingKnowledge.Setting.VerticalFov;
                
                if (sensible.Value.Distance < sensingKnowledge.Setting.FeelRange)
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
                    var forward = knowledge.AiOwnerEntity.Forward;
                    if (knowledge.EyeTransform != null)
                    {
                        forward = knowledge.EyeTransform.forward;
                           
                    }
                    var horizontalDirection = sensible.Value.Direction;
                    horizontalDirection.y = forward.y;
                    var horizontalAngle = Vector3.Angle(knowledge.AiOwnerEntity.Forward, horizontalDirection);

                    if (horizontalAngle < halfHorizontalFov)
                    {
                        var verticalDirection = sensible.Value.Direction;
                        verticalDirection.x = forward.x;
                        var verticalAngle = Vector3.Angle(knowledge.AiOwnerEntity.Forward, verticalDirection);
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
        /// 投掷物用, 同样会添加ThreatInfo
        /// </summary>
        /// <param name="knowledge"></param>
        /// <param name="checkPos"></param>
        /// <returns></returns>
        public static bool CanSignalBeNoticed(AIKnowledge knowledge, Vector3 checkPos)
        {
            var sourcelessHitAttractionRange = knowledge.SensingKnowledge.Setting.SourcelessHitAttractionRange;
            var hearAttractionRange = knowledge.SensingKnowledge.Setting.HearAttractionRange;
            var selectRange = hearAttractionRange;
            if (sourcelessHitAttractionRange > 0)
                selectRange = sourcelessHitAttractionRange;

            var currentPos = knowledge.CurrentPos;

            float distanceToTarget = Vector3.Distance(currentPos, checkPos);

            if (distanceToTarget < selectRange)
                return true;

            return false;
        }
    }
}