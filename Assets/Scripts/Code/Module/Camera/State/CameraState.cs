namespace TaoTie
{
    public abstract class CameraState: IPriorityStackItem
    {
        public CameraStateData Data;

        public int Priority { get; set; }
        public abstract void Update();
        
    }
}