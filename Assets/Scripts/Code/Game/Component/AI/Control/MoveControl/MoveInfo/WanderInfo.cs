using UnityEngine;

namespace TaoTie
{
    public class WanderInfo: MoveInfoBase
    {
        public enum WanderStatus
        {
            Inactive = 0,
            Wandering = 1
        }
        
        public WanderStatus Status;
        public long NextAvailableTick;
        public Vector3 WanderToPosCandidate;
        public static WanderInfo Create()
        {
            var res = ObjectPool.Instance.Fetch<WanderInfo>();

            return res;
        }
        
        public override void UpdateInternal(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIComponent ai, AIManager aiManager)
        {
            if (taskHandler.currentState == LocoTaskState.Finished)
            {
                TriggerCD(aiKnowledge);
                Status = WanderStatus.Inactive;
            }
        }

        public override void Enter(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIManager aiManager)
        {
            ConfigAIWanderData data = aiKnowledge.WanderTactic.Data;

            float distanceFromCurrentMin = data.DistanceFromCurrentMin;
            float distanceFromCurrentMax = data.DistanceFromCurrentMax;
            float distanceFromBorn = aiKnowledge.WanderTactic.Data.DistanceFromBorn;

            float turnSpeed = data.TurnSpeedOverride;

            float randomDistance = Random.Range(distanceFromCurrentMin, distanceFromCurrentMax);

            Vector3 randomDirection = new Vector3(Random.insideUnitCircle.x, 0, Random.insideUnitCircle.y);
            randomDirection *= randomDistance;

            WanderToPosCandidate = new Vector3(aiKnowledge.CurrentPos.x + randomDirection.x, aiKnowledge.CurrentPos.y, aiKnowledge.CurrentPos.z + randomDirection.z);


            var bornPos = aiKnowledge.BornPos;
            bornPos.y = WanderToPosCandidate.y;

            if (Vector3.Distance(bornPos, WanderToPosCandidate) >= distanceFromBorn)
            {
                WanderToPosCandidate = new Vector3(aiKnowledge.BornPos.x + randomDirection.x, aiKnowledge.BornPos.y, aiKnowledge.BornPos.z + randomDirection.z);
            }

            AILocomotionHandler.ParamGoTo param = new AILocomotionHandler.ParamGoTo
            {
                targetPosition = WanderToPosCandidate,
                cannedTurnSpeedOverride = turnSpeed,
                speedLevel = (MotionFlag)data.SpeedLevel,
            };

            taskHandler.CreateGoToTask(param);


            Status = WanderStatus.Wandering;
        }

        public override void Leave(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIManager aiManager)
        {
            base.Leave(taskHandler, aiKnowledge,aiManager);
            Status = WanderStatus.Inactive;
            if (taskHandler.currentState == LocoTaskState.Running)
                taskHandler.currentState = LocoTaskState.Interrupted;
        }

        public bool InWanderArea(Vector3 bornPos, Vector3 checkPos, ConfigAIWanderData wanderSetting)
        {
            float distanceFormBorn = Vector3.Distance(bornPos, checkPos);
            if (distanceFormBorn > wanderSetting.DistanceFromBorn)
                return false;
            return true;
        }

        public void TriggerCD(AIKnowledge knowledge, bool byFail = false)
        {
            int cdMin = knowledge.WanderTactic.Data.CdMin;
            int cdMax = knowledge.WanderTactic.Data.CdMax;
            NextAvailableTick = GameTimerManager.Instance.GetTimeNow() + Random.Range(cdMin, cdMax);
        }
        public override void Dispose()
        {
            NextAvailableTick = 0;
            Status = default;
            WanderToPosCandidate = default;
        }

        public override void UpdateLoco(AILocomotionHandler handler, AITransform currentTransform, ref LocoTaskState state)
        {
            
        }
    }
}