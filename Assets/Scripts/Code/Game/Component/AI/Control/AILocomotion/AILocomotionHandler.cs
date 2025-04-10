using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class AILocomotionHandler
    {
        private AIKnowledge knowledge;
        private LocoBaseTask currentTask;
        private float? originalYawSpeed;
        
        public LocoTaskState CurrentState;
        public struct ParamGoTo
        {
            public bool Scripted;
            public Vector3 TargetPosition;
            public MotionFlag SpeedLevel;
            public float CannedTurnSpeedOverride;
            public bool DelayStopping;
            public bool Spacial;
            public bool SpacialRoll;
            public NavMeshUseType UseNavmesh;
            public bool ExactlyMove;
            
        }

        public struct ParamFacingMove
        {
            public Unit Anchor;
            public MotionFlag SpeedLevel;
            public MotionDirection MovingDirection;
            public float Duration;
        }

        public struct ParamSurroundDash
        {
            public Unit Anchor;
            public Vector3? AnchorFixedPoint;
            public MotionFlag SpeedLevel;
            public float CannedTurnSpeedOverride;
            public bool Spacial;
            public bool SpacialRoll;
            public bool Clockwise;
            public bool ReverseMoveDir;
            public float Radius;
            public bool DelayStopping;
        }

        public struct ParamRotation
        {
            public Vector3 TargetPosition;
        }

        public struct ParamFollowMove
        {
            public Unit Anchor;
            public bool UseMeleeSlot;
            public MotionFlag SpeedLevel;
            public float TurnSpeed;
            public float TargetAngle;
            public float StopDistance;
        }

        public AILocomotionHandler(AIKnowledge knowledge)
        {
            this.knowledge = knowledge;
            CurrentState = LocoTaskState.Finished;
        }

        public void RefreshTask(Vector3 position)
        {
            currentTask.RefreshTask(this, position);
            CurrentState = LocoTaskState.Running;
        }
        public void UpdateTasks(AITransform currentTransform)
        {
            if (CurrentState == LocoTaskState.Interrupted)
            {
                CurrentState = LocoTaskState.Finished;
            }

            if (CurrentState == LocoTaskState.Finished)
            {
                FinishTask();
            }

            if (CurrentState == LocoTaskState.Running)
            {
                currentTask.UpdateLoco(this, currentTransform, ref CurrentState);
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
            CurrentState = LocoTaskState.Running;
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

        public void CreateSurroundDashTask(ParamSurroundDash param)
        {
            
        }

        public void CreateRotationTask(ParamRotation param)
        {
            RotationTask rotationTask = new RotationTask();
            rotationTask.Init(knowledge, param);


            CreateTask_Internal(rotationTask);
        }

        public void CreateSnakelickMove(ParamGoTo param)
        {
            
        } 
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
            knowledge.Input.TryMove(Vector3.zero, MotionFlag.Idle);
        }
        
        public void UpdateMotionFlag(MotionFlag newSpeed, MotionDirection direction = MotionDirection.Forward)
        {
            if (newSpeed == MotionFlag.Idle)
            {
                knowledge.Input.TryMove(Vector3.zero, newSpeed, direction);
                return;
            }
            if(currentTask == null) return;
            var dir = currentTask.GetDestination() - knowledge.Entity.Position;
            knowledge.Input.TryMove(dir, newSpeed, direction);
        }

        public void UpdateTaskSpeed(MotionFlag newSpeed)
        {
            currentTask.UpdateLocoSpeed(newSpeed);
        }
        public void UpdateTurnSpeed(float speed)
        {
            // knowledge.Mover.RotateSpeed = speed;
        }

        public void ForceLookAt()
        {
            if (currentTask == null) return;
            knowledge.Mover?.ForceLookAt(currentTask.GetDestination());
        }
    }
}