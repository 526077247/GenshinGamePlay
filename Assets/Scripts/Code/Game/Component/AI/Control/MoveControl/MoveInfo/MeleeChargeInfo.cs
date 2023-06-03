using UnityEngine;

namespace TaoTie
{
    public class MeleeChargeInfo: MoveInfoBase
    {
        public const int RETRY_TIMES = 3; 
        public enum ChargeStatus
        {
            Inactive = 0,
            Charging = 1
        }
        
        public ChargeStatus Status; 
        public Vector3 CurDestination;
        private int retryTimes;
        public static MeleeChargeInfo Create()
        {
            var res = ObjectPool.Instance.Fetch<MeleeChargeInfo>();

            return res;
        }
        public override void Enter(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIManager aiManager)
        {
            if (Status == ChargeStatus.Inactive)
            {
                ConfigAIMeleeChargeData data = aiKnowledge.MeleeChargeTactic.Data;

                float stopDistance = data.StopDistance;
                bool useMeleeSlot = data.UseMeleeSlot;
                float turnSpeed = data.TurnSpeedOverride;

                AIMoveSpeedLevel speedLevel = data.SpeedLevel;

                AILocomotionHandler.ParamFollowMove param = new AILocomotionHandler.ParamFollowMove
                {
                    anchor = aiKnowledge.TargetKnowledge.TargetEntity,
                    useMeleeSlot = useMeleeSlot,
                    speedLevel = speedLevel,
                    turnSpeed = turnSpeed,
                    stopDistance = stopDistance
                };

                taskHandler.CreateFollowMoveTask(param);
                Status = ChargeStatus.Charging;
            }
        }

        public override void UpdateInternal(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIComponent lcai,
            AIManager aiManager)
        {
            if (Status == ChargeStatus.Charging)
            {
                if (aiKnowledge.TargetKnowledge.TargetEntity == null) return;
                ConfigAIMeleeChargeData data = aiKnowledge.MeleeChargeTactic.Data;
                float stopDistance = data.StopDistance;
                float distance = Vector3.Distance(aiKnowledge.CurrentPos,
                    aiKnowledge.TargetKnowledge.TargetEntity.Position);
                if (distance < stopDistance)
                {
                    Status = ChargeStatus.Inactive;
                }
            }
        }
        
        public override void UpdateLoco(AILocomotionHandler handler, AITransform currentTransform, ref LocoTaskState state)
        {
            
        }

        public override void Leave(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIManager aiManager)
        {
            base.Leave(taskHandler, aiKnowledge, aiManager);
            Status = ChargeStatus.Inactive;
        }

        public override void Dispose()
        {

        }
    }
}