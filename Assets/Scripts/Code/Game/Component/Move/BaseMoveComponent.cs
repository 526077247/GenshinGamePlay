namespace TaoTie
{
    public abstract class BaseMoveComponent:Component
    {
        public bool isStart { get; private set; }
        
        public bool isPause { get; private set; }
        
        public delegate void OnMoveStateChangeDelegate(bool isStart);
        public event OnMoveStateChangeDelegate onMoveStateChange; 
        
        public virtual void OnStart()
        {
            isStart = true;
            isPause = false;
            onMoveStateChange?.Invoke(true);
        }

        public virtual void OnStop()
        {
            isStart = false;
            isPause = false;
            onMoveStateChange?.Invoke(false);
        }

        public virtual void Pause()
        {
            if (!isPause)
            {
                isPause = true;
                onMoveStateChange?.Invoke(false);
            }
        }
        
        public virtual void Resume()
        {
            if (isPause)
            {
                isPause = false;
                onMoveStateChange?.Invoke(true);
            }
        }

    }
}