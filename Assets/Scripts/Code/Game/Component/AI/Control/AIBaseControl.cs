namespace TaoTie
{
    public abstract class AIBaseControl
    {
        protected AIKnowledge aiKnowledge;

        public AIBaseControl(AIKnowledge aiKnowledge)
        {
            this.aiKnowledge = aiKnowledge;
        }
    }
}