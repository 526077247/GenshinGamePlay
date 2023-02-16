namespace TaoTie
{
    public abstract class BrainModuleBase
    {

        protected AIKnowledge knowledge;
        protected long nextTick;

        public void Init(AIKnowledge aiKnowledge)
        {
            this.knowledge = aiKnowledge;
            InitInternal();
        }
        protected virtual void InitInternal()
        {
            
        }
        private bool IsTickValid() => default;

        protected virtual void UpdateMainThreadInternal()
        {
        }

        public void UpdateMainThread()
        {
            UpdateMainThreadInternal();
        }

        protected virtual void UpdateComputeThreadInternal()
        {
        }

        public void UpdateComputeThread()
        {
            UpdateComputeThreadInternal();
        }

        public void UpdateNextInterval()
        {
        }

        public void Clear()
        {
            ClearInternal();
        }

        protected virtual void ClearInternal()
        {
        }
    }
}