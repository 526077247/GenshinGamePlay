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
            public PathQueryTask pathQuery;
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

        public AILocomotionHandler(AIKnowledge knowledge, AIPathfindingUpdater pPathfinding)
        {
            aiKnowledge = knowledge;
            pathfinder = pPathfinding;
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
                aiKnowledge.Mover.TryMove(currentTask.GetDestination() - currentTransform.pos);
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
            if (param.pathQuery == null)
            {
                param.pathQuery = aiKnowledge.PathFindingKnowledge.CreatePathQueryTask(aiKnowledge.CurrentPos,
                    param.targetPosition, param.useNavmesh);
            }
            GoToTask goToTask = new GoToTask();
            goToTask.Init(aiKnowledge, param);
            CreateTask_Internal(goToTask);
        }
        public void CreateFacingMoveTask(ParamFacingMove param) {} 
        public void CreateSurroundDashTask(ParamSurroundDash param) {}

        public void CreateRotationTask(ParamRotation param)
        {
            RotationTask rotationTask = new RotationTask();
            rotationTask.Init(aiKnowledge, param);


            CreateTask_Internal(rotationTask);
        } 
        public void CreateSnakelickMove(ParamGoTo param) {} 
        public void CreateFollowMoveTask(ParamFollowMove param)
        {
            FollowMoveTask followMoveTask = new FollowMoveTask();
            followMoveTask.Init(aiKnowledge, param);


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
                currentTask.OnCloseTask(this);
            aiKnowledge.Mover.TryMove(Vector3.zero);
        }
        
        public void UpdateMotionFlag(AIMoveSpeedLevel newSpeed)
        {
            if(!currentTask.stopped)
                aiKnowledge.Mover.ForceLookAt(currentTask.GetDestination());
            aiKnowledge.Mover.TryMove(currentTask.GetDestination() - aiKnowledge.AiOwnerEntity.Position, (MotionFlag)newSpeed);
        }

        public void UpdateTaskSpeed(AIMoveSpeedLevel newSpeed)
        {
            currentTask.UpdateLocoSpeed(newSpeed);
        }
        public void UpdateTurnSpeed(float speed)
        {
            aiKnowledge.Mover.RotateSpeed = speed;
        }
    }
}