using System;

namespace TaoTie
{
    public abstract class CameraState: IPriorityStackItem, IDisposable
    {
        public abstract bool IsBlenderState { get; }
        public CameraStateData Data;

        public int Priority { get; set; }
        public abstract void Update();

        public abstract void Dispose();
    }
}