using UnityEngine;

namespace TaoTie
{
    public class FleeInfo: MoveInfoBase
    {
        public enum FleeStatus
        {
            Inactive = 0,
            Fleeing = 1,
            FleeFinish = 2,
            RotateToTarget = 3
        }
        
        public FleeStatus status;
        public Vector3? fleePoint;
        public long nextAvailableTick;
        public int fleeNumberRemaining;
        public const sbyte SAMPLE_COUNT = 9;
        public const float EXPAND_FLEE_ANGLE = 120f;
        
        public static FleeInfo Create()
        {
            FleeInfo res = ObjectPool.Instance.Fetch<FleeInfo>();
            return res;
        }

        public override void Dispose()
        {
            status = default;
            fleePoint = null;
        }

        public override void UpdateInternal(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIComponent lcai, AIManager aiManager)
        {
            if (status == FleeStatus.Inactive)
            {
                ConfigAIFleeData data = aiKnowledge.fleeTactic.data;

                FindFleePosition(aiKnowledge);

                AILocomotionHandler.ParamGoTo param = new AILocomotionHandler.ParamGoTo
                {
                    targetPosition = (Vector3)fleePoint,
                    cannedTurnSpeedOverride = data.turnSpeedOverride,
                    speedLevel = (AIMoveSpeedLevel)data.speedLevel,
                };

                taskHandler.CreateGoToTask(param);
                status = FleeStatus.Fleeing;
            }

            if (status == FleeStatus.Fleeing)
            {
                if (taskHandler.currentState == LocoTaskState.Finished)
                {
                    status = FleeStatus.FleeFinish;
                }
            }

            if (status == FleeStatus.FleeFinish)
            {
                if (aiKnowledge.fleeTactic.data.turnToTarget)
                {
                    if (taskHandler.currentState == LocoTaskState.Finished)
                    {
                        Unit target = aiKnowledge.targetKnowledge.targetEntity;
                        AILocomotionHandler.ParamRotation param = new AILocomotionHandler.ParamRotation
                        {
                            targetPosition = target.Position
                        };
                        taskHandler.CreateRotationTask(param);

                        status = FleeStatus.RotateToTarget;
                    }
                }
                else
                {
                    status = FleeStatus.Inactive;
                    TriggerCD(aiKnowledge);
                }
            }

            if (status == FleeStatus.RotateToTarget)
            {
                if (taskHandler.currentState == LocoTaskState.Finished)
                {
                    status = FleeStatus.Inactive;
                    TriggerCD(aiKnowledge);
                }
            }

        }

        public override void Enter(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIManager aiManager)
        {
            ConfigAIFleeData data = aiKnowledge.fleeTactic.data;

            FindFleePosition(aiKnowledge);

            AILocomotionHandler.ParamGoTo param = new AILocomotionHandler.ParamGoTo
            {
                targetPosition = (Vector3)fleePoint,
                cannedTurnSpeedOverride = data.turnSpeedOverride,
                speedLevel = (AIMoveSpeedLevel)data.speedLevel,
            };

            taskHandler.CreateGoToTask(param);
            TriggerCD(aiKnowledge);
            status = FleeStatus.Fleeing;
        }

        public override void Leave(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIManager aiManager)
        {
            base.Leave(taskHandler, aiKnowledge,aiManager);
            TriggerCD(aiKnowledge);
            status = FleeStatus.Inactive;
        }


        public void TriggerCD(AIKnowledge knowledge, bool byFail = false)
        {
            nextAvailableTick = knowledge.fleeTactic.data.cd + GameTimerManager.Instance.GetTimeNow();
        }

        //TODO Add AIComponent param
        private bool FindFleePosition(AIKnowledge knowledge)
        {
            Vector3 enemyPos = knowledge.targetKnowledge.targetPosition;

            float fleeDistanceMin = knowledge.fleeTactic.data.fleeDistanceMin;
            float fleeDistanceMax = knowledge.fleeTactic.data.fleeDistanceMax;

            float randomDistance = Random.Range(fleeDistanceMin, fleeDistanceMax);

            float fleeAngle = knowledge.fleeTactic.data.fleeAngle;
            float randomAngle = Random.Range(fleeAngle * -0.5f, fleeAngle * 0.5f);

            Vector3 fleeDirection = knowledge.currentPos - enemyPos;
            fleeDirection.y = 0;
            fleeDirection = fleeDirection.normalized;

            fleeDirection = Quaternion.AngleAxis(randomAngle, knowledge.aiOwnerEntity.Up) * fleeDirection * randomDistance;

            fleePoint = knowledge.currentPos + fleeDirection;

            return true;
        }

        public override void UpdateLoco(AILocomotionHandler handler, AITransform currentTransform, ref LocoTaskState state)
        {
            
        }
    }
}