using UnityEngine;

namespace TaoTie
{
    public class CombatFollowMoveInfo: MoveInfoBase
    {
        public enum CombatFollowMoveStatus
        {
            Inactive = 0,
            CloseTo = 1
        }
        
        public CombatFollowMoveStatus Status;
        // private int retryTimes;
        public static CombatFollowMoveInfo Create()
        {
            return ObjectPool.Instance.Fetch<CombatFollowMoveInfo>();
        }
        public override void Enter(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIManager aiManager)
        {
            if (Status == CombatFollowMoveStatus.Inactive)
            {
                ConfigAIMeleeChargeData data = aiKnowledge.MeleeChargeTactic.Data;

                float stopDistance = data.StopDistance;
                bool useMeleeSlot = data.UseMeleeSlot;
                float turnSpeed = data.TurnSpeedOverride;

                MotionFlag speedLevel = data.SpeedLevel;

                AILocomotionHandler.ParamFollowMove param = new AILocomotionHandler.ParamFollowMove
                {
                    Anchor = aiKnowledge.TargetKnowledge.TargetEntity,
                    UseMeleeSlot = useMeleeSlot,
                    SpeedLevel = speedLevel,
                    TurnSpeed = turnSpeed,
                    StopDistance = stopDistance
                };

                // taskHandler.CreateFollowMoveTask(param);
                Status = CombatFollowMoveStatus.CloseTo;
            }
        }

        public override void UpdateInternal(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIComponent lcai,
            AIManager aiManager)
        {
            
        }

        public override void Leave(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIManager aiManager)
        {
            base.Leave(taskHandler, aiKnowledge, aiManager);
            Status = CombatFollowMoveStatus.Inactive;
        }

        public override void UpdateLoco(AILocomotionHandler handler, AITransform currentTransform, ref LocoTaskState state)
        {
            
        }

        public override void Dispose()
        {
            ObjectPool.Instance.Recycle(this);
        }
    }
}