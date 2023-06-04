using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class AILocomotionHandler
    {
        public AIKnowledge knowledge;
        public LocoBaseTask currentTask;
        public LocoTaskState currentState;
        private float? _originalYawSpeed;
        
        public struct ParamGoTo
        {
            public bool scripted;
            public Vector3 targetPosition;
            public MotionFlag speedLevel;
            public LocoBaseTask.ObstacleHandling obstacleHandling;
            public float cannedTurnSpeedOverride;
            public bool delayStopping;
            public bool spacial;
            public bool spacialRoll;
            public NavMeshUseType useNavmesh;
            public bool exactlyMove;
            
        }

        public struct ParamFacingMove
        {
            public Unit anchor;
            public MotionFlag speedLevel;
            public MotionDirection movingDirection;
            public float duration;
        }

        public struct ParamSurroundDash
        {

            public Entity anchorEntity;
            public Vector3? anchorFixedPoint;
            public MotionFlag speedLevel;
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
            public MotionFlag speedLevel;
            public float turnSpeed;
            public float targetAngle;
            public float stopDistance;
        }

        public AILocomotionHandler(AIKnowledge knowledge)
        {
            this.knowledge = knowledge;
            currentState = LocoTaskState.Finished;
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
                currentState = LocoTaskState.Finished;
            }

            if (currentState == LocoTaskState.Finished)
            {
                FinishTask();
            }

            if (currentState == LocoTaskState.Running)
            {
                currentTask.UpdateLoco(this, currentTransform, ref currentState);
                knowledge.Mover.TryMove(currentTask.GetDestination() - currentTransform.pos);
            }

        }
        
        #region CreateTask
        private void CreateTask_Internal(LocoBaseTask task/*, bool delayStopping, float? movingYawSpeedOverride*/)
        {
            if (currentTask != null)
            {
                ClearTask();
            }
            currentTask = task;
            currentState = LocoTaskState.Running;
        }

        public void CreateGoToTask(ParamGoTo param)
        {
            GoToTask goToTask = new GoToTask();
            goToTask.Init(knowledge, param);
            CreateTask_Internal(goToTask);
        }

        public void CreateFacingMoveTask(ParamFacingMove param)
        {
            FacingMoveTask facingMoveTask = new FacingMoveTask();
            facingMoveTask.Init(knowledge, param);


            CreateTask_Internal(facingMoveTask);
        } 
        public void CreateSurroundDashTask(ParamSurroundDash param) {}

        public void CreateRotationTask(ParamRotation param)
        {
            RotationTask rotationTask = new RotationTask();
            rotationTask.Init(knowledge, param);


            CreateTask_Internal(rotationTask);
        } 
        public void CreateSnakelickMove(ParamGoTo param) {} 
        public void CreateFollowMoveTask(ParamFollowMove param)
        {
            FollowMoveTask followMoveTask = new FollowMoveTask();
            followMoveTask.Init(knowledge, param);


            CreateTask_Internal(followMoveTask);
        }
        #endregion
        public bool TaskEnded()
        {
            return currentTask == null;
        }

        public void ClearTask()
        {
            currentTask.OnCloseTask(this);
            currentTask = null;
        }
        
        public void FinishTask() 
        {
            if (currentTask != null)
            {
                currentTask.OnCloseTask(this);
                currentTask = null;
            }
            knowledge.Mover.TryMove(Vector3.zero);
        }
        
        public void UpdateMotionFlag(MotionFlag newSpeed, MotionDirection direction = MotionDirection.Forward)
        {
            if (newSpeed == MotionFlag.Idle)
            {
                knowledge.Mover.TryMove(Vector3.zero);
                return;
            }
            if(currentTask==null) return;
            knowledge.Mover.TryMove(currentTask.GetDestination() - knowledge.Entity.Position, newSpeed, direction);
        }

        public void UpdateTaskSpeed(MotionFlag newSpeed)
        {
            currentTask.UpdateLocoSpeed(newSpeed);
        }
        public void UpdateTurnSpeed(float speed)
        {
            knowledge.Mover.RotateSpeed = speed;
        }

        public void ForceLookAt()
        {
            if(currentTask==null) return;
            knowledge.Mover.ForceLookAt(currentTask.GetDestination());
        }
    }
}