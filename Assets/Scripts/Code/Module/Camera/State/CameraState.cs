using System;

namespace TaoTie
{
    public abstract class CameraState: IPriorityStackItem, IDisposable
    {
        public abstract bool IsBlenderState { get; }
        public bool IsOver { get; set; }
        public CameraStateData Data;

        public long Id { get; set; }
        public int Priority { get; protected set; }
        public abstract void Update();

        public abstract void Dispose();
    }
}