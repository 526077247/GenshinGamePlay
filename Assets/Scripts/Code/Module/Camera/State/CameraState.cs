using System;

namespace TaoTie
{
    public abstract class CameraState: IPriorityStackItem, IDisposable
    {
        public abstract bool IsBlenderState { get; }
        public bool IsBackground { get; protected set; }
        public bool IsDispose { get; private set; }
        public bool IsOver { get; set; }
        public CameraStateData Data;

        public long Id { get; set; }
        public int Priority { get; protected set; }
       

        public virtual void OnEnter()
        {
            IsBackground = false;
        }

        public virtual void OnLeave()
        {
            if(IsDispose) return;
            IsBackground = true;
        }
        public abstract void Update();

        public virtual void Dispose()
        {
            if(IsDispose) return;
            OnLeave();
            IsDispose = true;
            CameraManager.Instance.RemoveState(Id);
            IsBackground = true;
            IsOver = true;
            Data?.Dispose();
            Data = null;
            Id = default;
            Priority = default;
        }
    }
}