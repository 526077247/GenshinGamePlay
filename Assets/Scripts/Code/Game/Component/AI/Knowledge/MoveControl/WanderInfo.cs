using UnityEngine;

namespace TaoTie
{
    public class WanderInfo: MoveInfoBase
    {
        public WanderStatus status;
        public long nextAvailableTick;
        public Vector3 wanderToPosCandidate;
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
                status = WanderStatus.Inactive;
            }
        }

        public override void Enter(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIManager aiManager)
        {
            ConfigAIWanderData data = aiKnowledge.wanderTactic.data;

            float distanceFromCurrentMin = data.DistanceFromCurrentMin;
            float distanceFromCurrentMax = data.DistanceFromCurrentMax;
            float distanceFromBorn = aiKnowledge.wanderTactic.data.DistanceFromBorn;

            float turnSpeed = data.TurnSpeedOverride;

            float randomDistance = Random.Range(distanceFromCurrentMin, distanceFromCurrentMax);

            Vector3 randomDirection = new Vector3(Random.insideUnitCircle.x, 0, Random.insideUnitCircle.y);
            randomDirection *= randomDistance;

            wanderToPosCandidate = new Vector3(aiKnowledge.currentPos.x + randomDirection.x, aiKnowledge.currentPos.y, aiKnowledge.currentPos.z + randomDirection.z);


            var bornPos = aiKnowledge.bornPos;
            bornPos.y = wanderToPosCandidate.y;

            if (Vector3.Distance(bornPos, wanderToPosCandidate) >= distanceFromBorn)
            {
                wanderToPosCandidate = new Vector3(aiKnowledge.bornPos.x + randomDirection.x, aiKnowledge.bornPos.y, aiKnowledge.bornPos.z + randomDirection.z);
            }

            AILocomotionHandler.ParamGoTo param = new AILocomotionHandler.ParamGoTo
            {
                targetPosition = wanderToPosCandidate,
                cannedTurnSpeedOverride = turnSpeed,
                speedLevel = (AIMoveSpeedLevel)data.SpeedLevel,
            };

            taskHandler.CreateGoToTask(param);


            status = WanderStatus.Wandering;
        }

        public override void Leave(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIManager aiManager)
        {
            base.Leave(taskHandler, aiKnowledge,aiManager);
            status = WanderStatus.Inactive;
            //if (taskHandler.currentState == AILocoTaskState.Running)
            //    taskHandler.currentState = AILocoTaskState.Interrupted;
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
            int cdMin = knowledge.wanderTactic.data.CdMin;
            int cdMax = knowledge.wanderTactic.data.CdMax;
            nextAvailableTick = GameTimerManager.Instance.GetTimeNow() + Random.Range(cdMin, cdMax);
        }
        public override void Dispose()
        {

        }

        public override void UpdateLoco(AILocomotionHandler handler, AITransform currentTransform, ref LocoTaskState state)
        {
            
        }
    }
}