namespace TaoTie
{
    public abstract class AIBaseControl
    {
        protected AIKnowledge aiKnowledge;

        public void Init(AIKnowledge aiKnowledge)
        {
            this.aiKnowledge = aiKnowledge;
            InitInternal();
        }
        protected virtual void InitInternal()
        {
            
        }
    }
}