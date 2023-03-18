using UnityEngine;

namespace TaoTie
{
    public class MeleeChargeInfo: MoveInfoBase
    {
        public enum ChargeStatus
        {
            Inactive = 0,
            Charging = 1
        }

        public const int RETRY_TIMES = 3; 
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
                ConfigAIMeleeChargeData data = aiKnowledge.meleeChargeTactic.data;

                float stopDistance = data.stopDistance;
                bool useMeleeSlot = data.useMeleeSlot;
                float turnSpeed = data.turnSpeedOverride;

                AIMoveSpeedLevel speedLevel = data.speedLevel;

                AILocomotionHandler.ParamFollowMove param = new AILocomotionHandler.ParamFollowMove
                {
                    anchor = aiKnowledge.targetKnowledge.targetEntity,
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