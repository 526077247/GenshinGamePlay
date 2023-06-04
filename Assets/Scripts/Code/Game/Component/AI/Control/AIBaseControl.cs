namespace TaoTie
{
    public abstract class AIBaseControl
    {
        protected AIKnowledge knowledge;

        public void Init(AIKnowledge aiKnowledge)
        {
            this.knowledge = aiKnowledge;
            InitInternal();
        }
        protected virtual void InitInternal()
        {
            
        }
    }
}