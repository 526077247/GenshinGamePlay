﻿using System;

namespace TaoTie
{
    public partial class FsmState : IDisposable
    {
        private Fsm fsm;
        private ConfigFsmState config;

        public Fsm Fsm => fsm;
        public string Name => config.Name;
        public ConfigFsmState Config => config;

        public float StateTime;
        public float StateNormalizedTime;
        public float StateElapseTime;
        public float StatePassTime;
        public float StateExitTime;
        
        
        public bool CanMove
        {
            get
            {
                if (Config?.Data == null) return true;
                return Config.Data.CanMove;
            }
        }
        
        public bool CanTurn
        {
            get
            {
                if (Config?.Data == null) return true;
                return Config.Data.CanTurn;
            }
        }

        public bool CanJump
        {
            get
            {
                if (Config?.Data == null) return true;
                return Config.Data.CanJump;
            }
        }
        public bool IsJump
        {
            get
            {
                if (Config?.Data == null) return false;
                return Config.Data.IsJump;
            }
        }
        public bool ShowWeapon
        {
            get
            {
                if (Config?.Data == null) return false;
                return Config.Data.ShowWeapon;
            }
        }

        public bool UseRagDoll
        {
            get
            {
                if (Config?.Data == null) return false;
                return Config.Data.UseRagDoll;
            }
        }
        public bool EffectBySpeed
        {
            get
            {
                if (Config?.Data == null) return false;
                return Config.Data.EffectBySpeed;
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
            this.fsm = fsm;
            config = cfg;
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
            fsm = null;
            config = null;
            ObjectPool.Instance.Recycle(this);
        }
        #endregion
    }
}