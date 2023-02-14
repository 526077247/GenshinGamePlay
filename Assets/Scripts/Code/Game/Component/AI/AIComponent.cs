using System.Collections.Generic;

namespace TaoTie
{
    public class AIComponent: Component,IComponent<ConfigAIBeta>,IUpdateComponent
    {
        public ConfigAIBeta Config { get; private set; }
        #region IComponent

        public virtual void Init(ConfigAIBeta p1)
        {
            Config = p1;
            
        }

        public virtual void Destroy()
        {
            Config = null;
        }

        public virtual void Update()
        {
           
            
        }

        #endregion


    }
}