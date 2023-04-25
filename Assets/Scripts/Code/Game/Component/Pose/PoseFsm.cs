namespace TaoTie
{
    public class PoseFsm: Fsm
    {
        private NumericComponent numericComponent => _component.GetParent<Entity>().GetComponent<NumericComponent>();
        private int poseID;
        public int PoseID => poseID;
        
        public delegate void PoseChangedDelegate(int from, int to);
        public PoseChangedDelegate onPoseChanged;
        public static PoseFsm Create(PoseFSMComponent ctrl, ConfigFsm cfg)
        {
            PoseFsm ret = ObjectPool.Instance.Fetch<PoseFsm>();
            ret.Init(ctrl, cfg);
            return ret;
        }

        protected override void Init(FsmComponent ctrl, ConfigFsm cfg)
        {
            base.Init(ctrl, cfg);
            Messager.Instance.AddListener<NumericChange>(ctrl.Id, MessageId.NumericChangeEvt, OnNumericChange);
        }

        public override void Dispose()
        {
            base.Dispose();
            Messager.Instance.RemoveListener<NumericChange>(_component.Id, MessageId.NumericChangeEvt, OnNumericChange);
        }
        private void OnNumericChange(NumericChange evt)
        {
            if (currentState != null && currentState.Config.data != null && currentState.Config.data.IsDynamic 
                && evt.NumericType == NumericType.GetKey(currentState.Config.data.Key))
            {
                var poseID = numericComponent.GetAsInt(evt.NumericType);
                InvokeOnPoseChanged(this.poseID, poseID);
            }
        }
        
        protected override void InvokeOnStateChanged(FsmState fromState, FsmState toState)
        {
            var poseID = toState?.Config == null ? 0 : GetPoseID(toState.Config);
            InvokeOnPoseChanged(this.poseID, poseID);
            base.InvokeOnStateChanged(fromState, toState);
        }

        private int GetPoseID(ConfigFsmState state)
        {
            if (state.data != null)
            {
                if (state.data.IsDynamic && numericComponent!=null)
                {
                    return numericComponent.GetAsInt(NumericType.GetKey(state.data.Key));
                }
                else
                {
                    return state.data.PoseID;
                }
            }

            return 0;
        }

        private void InvokeOnPoseChanged(int from, int to)
        {
            if (from != to)
            {
                poseID = to;
                onPoseChanged?.Invoke(from, poseID);
            }
        }

    }
}