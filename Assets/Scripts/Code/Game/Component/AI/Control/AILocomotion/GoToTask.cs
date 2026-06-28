using UnityEngine;

namespace TaoTie
{
    public class GoToTask: LocoBaseTask
    {
        public enum GoToTaskState
        {
            QueryPathfinder = 0,
            WaitPathfinder = 1,
            Moving = 2
        }
        
        protected GoToTaskState innerState;

        protected float getCloseDistance;
        protected float turnSpeed;
        protected PathQueryTask pathQuery;
        protected int index;
        protected NavMeshUseType useNavMesh;
        public void Init(AIKnowledge knowledge, AILocomotionHandler.ParamGoTo param)
        {
            base.Init(knowledge);
            speedLevel = param.SpeedLevel;
            destination = param.TargetPosition;
            useNavMesh = param.UseNavmesh;
            getCloseDistance = knowledge.MoveKnowledge.GetAlmostReachDistance(param.SpeedLevel);
            turnSpeed = param.CannedTurnSpeedOverride;
            innerState = GoToTaskState.QueryPathfinder;
            index = 0;
        }
        public override void UpdateLoco(AILocomotionHandler handler, AITransform currentTransform, ref LocoTaskState state)
        {
            if (innerState == GoToTaskState.QueryPathfinder)
            {
                if(pathQuery!=null) pathQuery.Dispose();
                pathQuery = knowledge.PathFindingKnowledge.CreatePathQueryTask(currentTransform.Position, destination, useNavMesh);
                innerState = GoToTaskState.WaitPathfinder;
            }

            if (innerState == GoToTaskState.WaitPathfinder)
            {
                if (pathQuery != null)
                {
                    if (pathQuery.Status == QueryStatus.Success)
                    {
                        innerState = GoToTaskState.Moving;
                        index = 0;
                        destination = pathQuery.Corners[index];
                        handler.ForceLookAt();
                    }
                    else if (pathQuery.Status == QueryStatus.Fail)
                    {
                        Stopped = true;
                        state = LocoTaskState.Finished;
                        handler.UpdateMotionFlag(MotionFlag.Idle);
                    }
                }
                else
                {
                    Log.Error("PathfinderTask == null");
                }
            }

            if (innerState == GoToTaskState.Moving)
            {
                var transfomPos = currentTransform.Position;
                transfomPos.y = 0;
                var target = destination;
                target.y = 0;

                if (!Stopped)
                {
                    var currentDistance = Vector3.Distance(transfomPos, target);
                    if (pathQuery!=null && pathQuery.Corners!=null && currentDistance <= getCloseDistance)
                    {
                        index++;
                        if (index >= pathQuery.Corners.Count)
                        {
                            Stopped = true;
                            state = LocoTaskState.Finished;
                            handler.UpdateMotionFlag(MotionFlag.Idle);
                        }
                        else
                        {
                            destination = pathQuery.Corners[index];
                            handler.ForceLookAt();
                            handler.UpdateMotionFlag(speedLevel);
                            if (turnSpeed != 0)
                            {
                                handler.UpdateTurnSpeed(turnSpeed);
                            }
                        }
                    }
                    else
                    {
                        OnMovingToDestination(handler, transfomPos, target);
                    }
                }
                else
                {
                    state = LocoTaskState.Finished;
                    handler.UpdateMotionFlag(MotionFlag.Idle);
                }
            }
        }

        /// <summary>
        /// 朝路径点移动时的处理，子类可重写以实现特殊移动模式
        /// </summary>
        protected virtual void OnMovingToDestination(AILocomotionHandler handler, Vector3 currentPos, Vector3 target)
        {
            handler.UpdateMotionFlag(speedLevel);
            if (turnSpeed != 0)
            {
                handler.UpdateTurnSpeed(turnSpeed);
            }
        }
        public override void RefreshTask(AILocomotionHandler handler, Vector3 positoin)
        {
            Stopped = false;
            destination = positoin;
            innerState = GoToTaskState.QueryPathfinder;
        }

        public override void OnCloseTask(AILocomotionHandler handler)
        {
            base.OnCloseTask(handler);
            if (pathQuery != null)
            {
                pathQuery.Dispose();
                pathQuery = null;
            }
        }
    }
}