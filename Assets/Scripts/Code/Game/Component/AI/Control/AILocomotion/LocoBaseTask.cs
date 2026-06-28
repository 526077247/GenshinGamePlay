using UnityEngine;

namespace TaoTie
{
    public abstract class LocoBaseTask
    {
        protected Vector3 destination;
        protected MotionFlag speedLevel;
        protected long startTick;
        protected AIKnowledge knowledge;

        public bool Stopped { get; protected set; }

        public abstract void UpdateLoco(AILocomotionHandler handler, AITransform currentTransform, ref LocoTaskState state);

        public virtual void UpdateLocoSpeed(MotionFlag speed)
        {
            this.speedLevel = speed;
        }

        public virtual Vector3 GetDestination() => destination;

        public virtual void OnCloseTask(AILocomotionHandler handler)
        {
            Stopped = true;
            handler.UpdateMotionFlag(MotionFlag.Idle);
        }
        
        protected void Init(AIKnowledge knowledge)
        {
            this.knowledge = knowledge;
            this.startTick = GameTimerManager.Instance.GetTimeNow();
        }

        public virtual void RefreshTask(AILocomotionHandler handler, Vector3 positoin)
        {
            
        }
    }
}