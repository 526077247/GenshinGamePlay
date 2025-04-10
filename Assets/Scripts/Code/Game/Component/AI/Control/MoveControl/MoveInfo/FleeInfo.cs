using UnityEngine;

namespace TaoTie
{
    public class FleeInfo: MoveInfoBase
    {
        public const sbyte SAMPLE_COUNT = 9;
        public const float EXPAND_FLEE_ANGLE = 120f;
        
        public enum FleeStatus
        {
            Inactive = 0,
            Fleeing = 1,
            FleeFinish = 2,
            RotateToTarget = 3
        }
        
        public FleeStatus Status;
        public Vector3? FleePoint;
        public long NextAvailableTick;
        public int FleeNumberRemaining;
        
        public static FleeInfo Create()
        {
            return ObjectPool.Instance.Fetch<FleeInfo>();
        }

        public override void Dispose()
        {
            Status = default;
            FleePoint = null;
            ObjectPool.Instance.Recycle(this);
        }

        public override void UpdateInternal(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIComponent lcai, AIManager aiManager)
        {
            if (Status == FleeStatus.Inactive)
            {
                ConfigAIFleeData data = aiKnowledge.FleeTactic.Data;

                FindFleePosition(aiKnowledge);

                AILocomotionHandler.ParamGoTo param = new AILocomotionHandler.ParamGoTo
                {
                    TargetPosition = (Vector3)FleePoint,
                    CannedTurnSpeedOverride = data.TurnSpeedOverride,
                    SpeedLevel = (MotionFlag)data.SpeedLevel,
                };

                taskHandler.CreateGoToTask(param);
                Status = FleeStatus.Fleeing;
            }

            if (Status == FleeStatus.Fleeing)
            {
                if (taskHandler.CurrentState == LocoTaskState.Finished)
                {
                    Status = FleeStatus.FleeFinish;
                }
            }

            if (Status == FleeStatus.FleeFinish)
            {
                if (aiKnowledge.FleeTactic.Data.TurnToTarget)
                {
                    if (taskHandler.CurrentState == LocoTaskState.Finished)
                    {
                        Unit target = aiKnowledge.TargetKnowledge.TargetEntity;
                        AILocomotionHandler.ParamRotation param = new AILocomotionHandler.ParamRotation
                        {
                            TargetPosition = target.Position
                        };
                        taskHandler.CreateRotationTask(param);

                        Status = FleeStatus.RotateToTarget;
                    }
                }
                else
                {
                    Status = FleeStatus.Inactive;
                    TriggerCD(aiKnowledge);
                }
            }

            if (Status == FleeStatus.RotateToTarget)
            {
                if (taskHandler.CurrentState == LocoTaskState.Finished)
                {
                    Status = FleeStatus.Inactive;
                    TriggerCD(aiKnowledge);
                }
            }

        }

        public override void Enter(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIManager aiManager)
        {
            ConfigAIFleeData data = aiKnowledge.FleeTactic.Data;

            FindFleePosition(aiKnowledge);

            AILocomotionHandler.ParamGoTo param = new AILocomotionHandler.ParamGoTo
            {
                TargetPosition = (Vector3)FleePoint,
                CannedTurnSpeedOverride = data.TurnSpeedOverride,
                SpeedLevel = (MotionFlag)data.SpeedLevel,
            };

            taskHandler.CreateGoToTask(param);
            TriggerCD(aiKnowledge);
            Status = FleeStatus.Fleeing;
        }

        public override void Leave(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIManager aiManager)
        {
            base.Leave(taskHandler, aiKnowledge,aiManager);
            TriggerCD(aiKnowledge);
            Status = FleeStatus.Inactive;
        }


        public void TriggerCD(AIKnowledge knowledge, bool byFail = false)
        {
            NextAvailableTick = knowledge.FleeTactic.Data.CD + GameTimerManager.Instance.GetTimeNow();
        }

        //TODO Add AIComponent param
        private bool FindFleePosition(AIKnowledge knowledge)
        {
            Vector3 enemyPos = knowledge.TargetKnowledge.TargetPosition;

            float fleeDistanceMin = knowledge.FleeTactic.Data.FleeDistanceMin;
            float fleeDistanceMax = knowledge.FleeTactic.Data.FleeDistanceMax;

            float randomDistance = Random.Range(fleeDistanceMin, fleeDistanceMax);

            float fleeAngle = knowledge.FleeTactic.Data.FleeAngle;
            float randomAngle = Random.Range(fleeAngle * -0.5f, fleeAngle * 0.5f);

            Vector3 fleeDirection = knowledge.CurrentPos - enemyPos;
            fleeDirection.y = 0;
            fleeDirection = fleeDirection.normalized;

            fleeDirection = Quaternion.AngleAxis(randomAngle, knowledge.Entity.Up) * fleeDirection * randomDistance;

            FleePoint = knowledge.CurrentPos + fleeDirection;

            return true;
        }

        public override void UpdateLoco(AILocomotionHandler handler, AITransform currentTransform, ref LocoTaskState state)
        {
            
        }
    }
}