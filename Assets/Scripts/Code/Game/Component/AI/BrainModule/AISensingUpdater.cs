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

        public AISensingUpdater(AIManager aiManager)
        {
            this.aiManager = aiManager;
        }

        protected override void InitInternal()
        {
            base.InitInternal();
            sensingKnowledge = knowledge.sensingKnowledge;
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
            CollectEnemies();
            ProcessEnemies();
        }

        private void CollectEnemies()
        {
            var entityList = aiManager.GetEnemies(knowledge.campID);
            foreach (var item in entityList)
            {
                foreach (var entity in item.Value)
                {
                    var entityID = entity.Id;
                    // var entityCampID = entity.campID;
                    var entityPos = entity.Position;
                    var selfPos = knowledge.aiOwnerEntity.Position + knowledge.aiOwnerEntity.Rotation*knowledge.eyePos;
                    var direction = (entityPos - selfPos).normalized;

                    if (enemySensiblesPreparation.TryGetValue(entityID, out var sensible))
                    {
                        sensible.distance = Vector3.Distance(entityPos, selfPos);
                        sensible.position = entityPos;
                        sensible.direction = direction;
                    }
                    else
                    {
                        SensibleInfo newSensible = new SensibleInfo()
                        {
                            sensibleID = entityID,
                            distance = Vector3.Distance(entityPos, selfPos),
                            position = entityPos,
                            direction = direction,
                        };
                        enemySensiblesPreparation.Add(newSensible.sensibleID, newSensible);
                        Log.Info("AI sensing updater new add entity id : {0}", newSensible.sensibleID);
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
                //var viewRange = knowledge.threatLevel == AIThreatLevel.Alert ? 200 : knowledge.sensingKnowledge.setting.viewRange;
                //var halfHorizontalFov = knowledge.sensingKnowledge.setting.viewPanoramic ? 180f : 0.5 * knowledge.sensingKnowledge.setting.horizontalFov;
                //var halfVerticalFov = knowledge.sensingKnowledge.setting.viewPanoramic ? 180f : 0.5 * knowledge.sensingKnowledge.setting.verticalFov;

                ////FeelRange
                //if (sensible.Value.distance < knowledge.sensingKnowledge.setting.feelRange)
                //{
                //    enemySensibles.TryAdd(sensible.Key, sensible.Value);

                //    //Fire Events
                //    Global.Event.Fire(SensibleCollected.Create(sensible.Value, AIThreatAddReason.Feel));
                //}

                ////VisionRange
                //else if (sensible.Value.distance < viewRange)
                //{
                //    var horizontalDirection = sensible.Value.direction;
                //    horizontalDirection.y = knowledge.eyeTransform.forward.y;
                //    var horizontalAngle = Vector3.Angle(knowledge.owner.transform.forward, horizontalDirection);

                //    if (horizontalAngle < halfHorizontalFov)
                //    {
                //        var verticalDirection = sensible.Value.direction;
                //        verticalDirection.x = knowledge.eyeTransform.forward.x;

                //        var verticalAngle = Vector3.Angle(knowledge.owner.transform.forward, verticalDirection);
                //        if (verticalAngle < halfVerticalFov)
                //        {
                //            //NLog.Info(LogConst.AI, "Collect Target ID : {0} by VISION.", sensible.Key);
                //            enemySensibles.TryAdd(sensible.Key, sensible.Value);

                //            //Fire Events
                //            Global.Event.Fire(SensibleCollected.Create(sensible.Value, AIThreatAddReason.Vision));
                //        }
                //    }
                //}
            }

            if (enemySensibles != null && enemySensibles.Count > 0)
            {
                var nearestEnemyID = enemySensibles.ElementAt(0).Value.sensibleID;
                float nearestEnemyDistance = enemySensibles.ElementAt(0).Value.distance;
                foreach (var enemy in enemySensibles)
                {
                    if (enemy.Value.distance < nearestEnemyDistance)
                    {
                        nearestEnemyDistance = enemy.Value.distance;
                        nearestEnemyID = enemy.Value.sensibleID;
                    }
                }
                knowledge.sensingKnowledge.nearestEnemy = nearestEnemyID;
                knowledge.sensingKnowledge.nearestEnemyDistance = nearestEnemyDistance;
            }
            else
            {
                //Reset nearest data
                knowledge.sensingKnowledge.nearestEnemy = 0;
                knowledge.sensingKnowledge.nearestEnemyDistance = -1f;
            }

            knowledge.sensingKnowledge.enemySensibles = enemySensibles;
        }

        //TODO 投掷物用, 同样会添加ThreatInfo
        public static bool CanSignalBeNoticed(AIKnowledge knowledge, Vector3 checkPos)
        {
            var sourcelessHitAttractionRange = knowledge.sensingKnowledge.setting.sourcelessHitAttractionRange;
            var hearAttractionRange = knowledge.sensingKnowledge.setting.hearAttractionRange;
            var selectRange = hearAttractionRange;
            if (sourcelessHitAttractionRange > 0)
                selectRange = sourcelessHitAttractionRange;

            var currentPos = knowledge.currentPos;

            float distanceToTarget = Vector3.Distance(currentPos, checkPos);

            if (distanceToTarget < selectRange)
                return true;

            return false;
        }
    }
}