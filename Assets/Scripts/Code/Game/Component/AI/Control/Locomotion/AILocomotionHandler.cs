using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class AILocomotionHandler
    {
        public AIKnowledge aiKnowledge;
        public AIPathfindingUpdater pathfinder;
        public LocoBaseTask currentTask;
        public LocoTaskState currentState;
        private float? _originalYawSpeed;
        
        public struct ParamGoTo
        {

            public bool scripted;
            public Vector3 targetPosition;
            public AIMoveSpeedLevel speedLevel;
            public List<Vector3> pathQuery;
            public LocoBaseTask.ObstacleHandling obstacleHandling;
            public float cannedTurnSpeedOverride;
            public bool delayStopping;
            public bool spacial;
            public bool spacialRoll;
            public NavMeshUseType useNavmesh;
            public bool exactlyMove;


            public enum NavMeshUseType
            {
                Auto = 0,
                ForceUse = 1,
                NotUse = 2
            }
        }

        public struct ParamFacingMove
        {
            public Entity anchor;
            public AIMoveSpeedLevel speedLevel;
            // public VCMoveData.MotionDirection movingDirection;
            public float duration;
        }

        public struct ParamSurroundDash
        {

            public Entity anchorEntity;
            public Vector3? anchorFixedPoint;
            public AIMoveSpeedLevel speedLevel;
            public float cannedTurnSpeedOverride;
            public bool spacial;
            public bool spacialRoll;
            public bool clockwise;
            public bool reverseMoveDir;
            public float radius;
            public bool delayStopping;
        }

        public struct ParamRotation
        {

            public Vector3 targetPosition;
        }

        public struct ParamFollowMove
        {

            public Entity anchor;
            public bool useMeleeSlot;
            public AIMoveSpeedLevel speedLevel;
            public float turnSpeed;
            public float targetAngle;
            public float stopDistance;
        }

    }
}