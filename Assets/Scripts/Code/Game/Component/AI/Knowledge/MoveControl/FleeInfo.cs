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
        public float nextAvailableTick;
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

        public override void Enter(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIManager aiManager)
        {
        }

        public override void UpdateInternal(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIComponent lcai,
            AIManager aiManager)
        {
        }
        
        public override void UpdateLoco(AILocomotionHandler handler, AITransform currentTransform, ref LocoTaskState state)
        {
            
        }
        
    }
}