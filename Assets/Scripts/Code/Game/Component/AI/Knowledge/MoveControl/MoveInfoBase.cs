using System;

namespace TaoTie
{
    public abstract class MoveInfoBase: IDisposable
    {
        public virtual void Enter(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIManager aiManager)
        {
            
        }

        public void UpdateMoveInfo(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIComponent ai, AIManager aiManager)
        {
            UpdateInternal(taskHandler, aiKnowledge, ai, aiManager);
        }
        public abstract void UpdateInternal(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIComponent lcai, AIManager aiManager);
        public abstract void UpdateLoco(AILocomotionHandler handler, AITransform currentTransform, ref LocoTaskState state);
        public virtual void Leave(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIManager aiManager)
        {
            
        }

        public virtual void Dispose()
        {

        }
    }
}