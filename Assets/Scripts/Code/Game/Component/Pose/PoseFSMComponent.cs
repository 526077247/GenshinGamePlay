using UnityEngine;

namespace TaoTie
{
    public class PoseFSMComponent: FsmComponent
    {
        public new PoseFsm DefaultFsm => base.DefaultFsm as PoseFsm;
        protected override Fsm CreateFsm(ConfigFsm fsmCfg)
        {
            return PoseFsm.Create(this, fsmCfg);
        }
    }
}