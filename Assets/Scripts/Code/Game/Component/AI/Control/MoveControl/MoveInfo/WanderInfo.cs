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
            return ObjectPool.Instance.Fetch<WanderInfo>();
        }
        
        public override void UpdateInternal(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIComponent ai, AIManager aiManager)
        {
            if (Status == WanderStatus.Wandering && taskHandler.CurrentState != LocoTaskState.Running)
            {
                if (!InWanderArea(aiKnowledge.CurrentPos, aiKnowledge.BornPos, aiKnowledge.WanderTactic.Data))
                {
                    StartNewTask(taskHandler, aiKnowledge);
                    return;
                }
            }
            
            var timeNow = GameTimerManager.Instance.GetTimeNow();
            if (timeNow > NextAvailableTick)
            {
                StartNewTask(taskHandler, aiKnowledge);
            }
            else if (taskHandler.CurrentState == LocoTaskState.Finished)
            {
                Status = WanderStatus.Inactive;
            }
        }

        private void StartNewTask(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge)
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
                TargetPosition = WanderToPosCandidate,
                CannedTurnSpeedOverride = turnSpeed,
                SpeedLevel = data.SpeedLevel,
            };

            taskHandler.CreateGoToTask(param);
            Status = WanderStatus.Wandering;
            TriggerCD(aiKnowledge);
        }

        public override void Enter(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIManager aiManager)
        {
            StartNewTask(taskHandler, aiKnowledge);
        }

        public override void Leave(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIManager aiManager)
        {
            base.Leave(taskHandler, aiKnowledge,aiManager);
            Status = WanderStatus.Inactive;
            if (taskHandler.CurrentState == LocoTaskState.Running)
                taskHandler.CurrentState = LocoTaskState.Interrupted;
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
            ObjectPool.Instance.Recycle(this);
        }
    }
}