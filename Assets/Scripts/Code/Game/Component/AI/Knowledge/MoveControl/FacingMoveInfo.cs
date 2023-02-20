using System.Collections.Generic;

namespace TaoTie
{
    public class FacingMoveInfo : MoveInfoBase
    {
        private float nextTickPickDirection;
        private float nextTickBackRaycast;
        private float nextTickMoveDirObstacleCheck;
        private bool isBackClear;
        private MotionDirection currentMoveDirection;
        private static List<MotionDirection> moveDirList;

        public static FacingMoveInfo Create()
        {
            FacingMoveInfo res = ObjectPool.Instance.Fetch<FacingMoveInfo>();
            return res;
        }

        public override void Dispose()
        {
        }

        public override void Enter(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIManager aiManager)
        {
        }

        public override void UpdateInternal(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIComponent lcai,
            AIManager aiManager)
        {
        }

        private MotionDirection GetNewMoveDirection(ConfigAIFacingMoveWeight weight) => default;

        private bool CheckLRHitWall(AIKnowledge aiKnowledge, float obstacleDetectRange) => default;
    }
}