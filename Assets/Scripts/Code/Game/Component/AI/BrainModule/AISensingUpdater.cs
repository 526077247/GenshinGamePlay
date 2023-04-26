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
        }


        protected override void ClearInternal()
        {
            base.ClearInternal();
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

                    if (enemySensiblesPreparation.TryGetValue(entityID, out var sensible))
                    {
                        sensible.Distance = Vector3.Distance(entityPos, selfPos);
                        sensible.Position = entityPos;
                        sensible.Direction = direction;
                    }
                    else
                    {
                        SensibleInfo newSensible = new SensibleInfo()
                        {
                            SensibleID = entityID,
                            Distance = Vector3.Distance(entityPos, selfPos),
                            Position = entityPos,
                            Direction = direction,
                        };
                        enemySensiblesPreparation.Add(newSensible.SensibleID, newSensible);
                        Log.Info("AI sensing updater new add entity id : {0}", newSensible.SensibleID);
                    }

                }
            }
        }

        private void ProcessEnemies()
        {
            enemySensibles.Clear();
            foreach (var sensible in enemySensiblesPreparation)
            {
                enemySensibles.TryAdd(sensible.Key, sensible.Value);
                var viewRange = knowledge.ThreatLevel == ThreatLevel.Alert ? 200 : sensingKnowledge.Setting.ViewRange;
                var halfHorizontalFov = sensingKnowledge.Setting.ViewPanoramic ? 180f : 0.5 * sensingKnowledge.Setting.HorizontalFov;
                var halfVerticalFov = sensingKnowledge.Setting.ViewPanoramic ? 180f : 0.5 * sensingKnowledge.Setting.VerticalFov;

                //FeelRange
                if (sensible.Value.Distance < sensingKnowledge.Setting.FeelRange)
                {
                    enemySensibles.TryAdd(sensible.Key, sensible.Value);
                }

                //VisionRange
                else if (sensible.Value.Distance < viewRange)
                {
                    var horizontalDirection = sensible.Value.Direction;
                    horizontalDirection.y = knowledge.EyeTransform.forward.y;
                    var horizontalAngle = Vector3.Angle(knowledge.AiOwnerEntity.Forward, horizontalDirection);

                    if (horizontalAngle < halfHorizontalFov)
                    {
                        var verticalDirection = sensible.Value.Direction;
                        verticalDirection.x = knowledge.EyeTransform.forward.x;

                        var verticalAngle = Vector3.Angle(knowledge.AiOwnerEntity.Forward, verticalDirection);
                        if (verticalAngle < halfVerticalFov)
                        {
                            enemySensibles.TryAdd(sensible.Key, sensible.Value);
                        }
                    }
                }
            }

            sensingKnowledge.NearestEnemy = 0;
            sensingKnowledge.NearestEnemyDistance = -1f;
            if (enemySensibles != null)
            {
                foreach (var enemy in enemySensibles)
                {
                    if (sensingKnowledge.NearestEnemyDistance<0 || enemy.Value.Distance < sensingKnowledge.NearestEnemyDistance)
                    {
                        sensingKnowledge.NearestEnemyDistance = enemy.Value.Distance;
                        sensingKnowledge.NearestEnemy = enemy.Value.SensibleID;
                    }
                }
            }
            sensingKnowledge.EnemySensibles = enemySensibles;
        }

        //TODO 投掷物用, 同样会添加ThreatInfo
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