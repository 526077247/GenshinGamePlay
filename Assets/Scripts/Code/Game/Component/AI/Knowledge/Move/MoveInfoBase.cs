﻿using System;

namespace TaoTie
{
    public abstract class MoveInfoBase: IDisposable
    {
        public virtual void Enter(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIManager aiManager)
        {
            
        }
        public abstract void UpdateInternal(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIComponent lcai, AIManager aiManager);
        
        public virtual void Leave(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIManager aiManager)
        {
            
        }

        public virtual void Dispose()
        {

        }
    }
}