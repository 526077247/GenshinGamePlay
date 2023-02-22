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
            public Unit anchor;
            public bool useMeleeSlot;
            public AIMoveSpeedLevel speedLevel;
            public float turnSpeed;
            public float targetAngle;
            public float stopDistance;
        }

        public void RefreshTask(Vector3 position)
        {
            currentTask.RefreshTask(this, position);
            currentState = LocoTaskState.Running;
        }
        public void UpdateTasks(AITransform currentTransform)
        {
            if (currentState == LocoTaskState.Interrupted)
            {
                if (!aiKnowledge.moveKnowledge.canFly)
                {
                    if (!aiKnowledge.moveKnowledge.inAir)
                    {
                        currentState = LocoTaskState.Finished;
                    }
                    else
                    {
                        // _timeOutTick -= GameTimerManager.Instance.GetDeltaTime();
                        // if (_timeOutTick <= 0f)
                        //     UpdateMotionFlag(AIMoveSpeedLevel.Idle);
                    }
                }
            }

            if (currentState == LocoTaskState.Finished)
            {
                // _timeOutTick = 0.2f;
                FinishTask();
            }

            if (currentState == LocoTaskState.Running)
            {
                currentTask.UpdateLoco(this, currentTransform, ref currentState);
            }

        }
        private void CreateTask_Internal(LocoBaseTask task/*, bool delayStopping, float? movingYawSpeedOverride*/)
        {
            if (currentTask != null)
            {
                ClearTask();
            }
            currentTask = task;
            currentState = LocoTaskState.Running;
        }

        public void CreateGoToTask(ParamGoTo param) {}
        public void CreateFacingMoveTask(ParamFacingMove param) {} 
        public void CreateSurroundDashTask(ParamSurroundDash param) {} 
        public void CreateRotationTask(ParamRotation param) {} 
        public void CreateSnakelickMove(ParamGoTo param) {} 
        public void CreateFollowMoveTask(ParamFollowMove param)
        {
            FollowMoveTask followMoveTask = new FollowMoveTask();
            followMoveTask.Init(aiKnowledge, param);


            CreateTask_Internal(followMoveTask);
        }

        public bool TaskEnded() => default;

        public void ClearTask() {}
        public void FinishTask() {}
        
        public void UpdateMotionFlag(AIMoveSpeedLevel newSpeed)
        {
            
        }
        
        public void UpdateTaskSpeed(AIMoveSpeedLevel newSpeed) {} 
        public void SetGroundFollowAnimationRotation(bool enabled) {} 
        public void Teleport(Vector3 targetPosition) {} 
        public void SwitchRotation(bool rotate) {} 
        public void SetForwardImmediately(Vector3 dir) {}
        public float GetDistanceToPathEnd(Vector3 currentPos) => default;
        public float GetAlmostReachedDistance() => default;
    }
}