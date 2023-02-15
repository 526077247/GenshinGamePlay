using System;

namespace TaoTie
{
    public abstract class MoveInfoBase: IDisposable
    {
        public virtual void Enter(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIComponent aiManager)
        {
            
        }

        public virtual void Leave(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIComponent aiManager)
        {
            
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}