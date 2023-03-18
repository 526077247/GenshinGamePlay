using System;

namespace TaoTie
{
    public partial class FsmState : IDisposable
    {
        private Fsm _fsm;
        private ConfigFsmState _config;

        public Fsm fsm => _fsm;
        public string name => _config.name;
        public ConfigFsmState config => _config;

        public bool CanMove
        {
            get
            {
                if (config.data == null) return true;
                return config.data.CanMove;
            }
        }
        
        public bool CanTurn
        {
            get
            {
                if (config.data == null) return true;
                return config.data.CanTurn;
            }
        }

        public static FsmState Create(Fsm fsm, ConfigFsmState cfg)
        {
            FsmState ret = ObjectPool.Instance.Fetch<FsmState>();
            ret.Init(fsm, cfg);
            return ret;
        }

        private void Init(Fsm fsm, ConfigFsmState cfg)
        {
            _fsm = fsm;
            _config = cfg;
        }

        public void OnEnter()
        {
            StartTimeline();
        }

        public void OnUpdate()
        {
            UpdateTimeline();
        }

        public void OnExit()
        {
            StopTimeline();
        }

        #region IDisposable
        public void Dispose()
        {
            ClearTimeline();
            _fsm = null;
            _config = null;
            ObjectPool.Instance.Recycle(this);
        }
        #endregion
    }
}