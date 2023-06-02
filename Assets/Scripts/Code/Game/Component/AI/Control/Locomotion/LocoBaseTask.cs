using UnityEngine;

namespace TaoTie
{
    public abstract class LocoBaseTask
    {
        public bool delayStopping;
        protected Vector3 destination;
        protected AIMoveSpeedLevel speedLevel;
        protected long startTick;
        protected ObstacleHandling obstacleHandling;
        protected AIKnowledge aiKnowledge;
        protected const float CHECKFAIL_PRE_TIME = 1.5f;
        protected Vector3? prevPos;
        protected AITimer moveFailTimer;
        protected float distanceMoved;
        protected Vector3? moveFailStartPos;
        public bool stopped { get; protected set; }
        protected DirectionLock directionLock;


        public enum ObstacleHandling
        {
            KeepMoving = 0,
            Stop = 1,
            StopOnlyByPathEnd = 2,
            Teleport = 3
        }

        public struct DirectionLock
        {

            public bool lockX;
            public bool lockY;
            public bool lockZ;


            public Vector3 Apply(Vector3 origin)
            {
                return new Vector3(lockX ? 0 : origin.x, lockY ? 0 : origin.y, lockZ ? 0 : origin.z);
            }
        }

        public abstract void UpdateLoco(AILocomotionHandler handler, AITransform currentTransform, ref LocoTaskState state);


        public virtual void UpdateLocoSpeed(AIMoveSpeedLevel speed)
        {
            this.speedLevel = speed;
        }

        public virtual Vector3 GetDestination() => destination;
        public virtual bool NeedPathfinder() => default;

        public virtual void OnCloseTask(AILocomotionHandler handler)
        {
            handler.UpdateMotionFlag(0);
        }

        public virtual void ShowPath() {}
        protected void Init(AIKnowledge knowledge)
        {
            this.aiKnowledge = knowledge;
            this.startTick = GameTimerManager.Instance.GetTimeNow();
        }

        public virtual void SetDirectionLock(DirectionLock dl)
        {
            directionLock = dl;
        }

        public virtual void RefreshTask(AILocomotionHandler handler, Vector3 positoin)
        {
            
        }
    }
}