using UnityEngine;

namespace TaoTie
{
    public class PoseFSMComponent: FsmComponent
    {
        public override Animator animator=> null;
        
        protected override Fsm CreateFsm(ConfigFsm fsmCfg)
        {
            return PoseFsm.Create(this, fsmCfg);
        }
    }
}