using System;
using UnityEngine;

namespace TaoTie
{
    public class FsmComponent: Component, IComponent<ConfigFsmController>,IUpdateComponent
    {
        private Fsm[] _fsms;
        private ConfigFsmController _config;
        protected VariableSet _variableSet;

        public virtual Animator animator => Parent.GetComponent<GameObjectHolderComponent>()?.Animator;
        public VariableSet variableSet => _variableSet;
        private ListComponent<ConfigParamTrigger> _triggers;
        public Fsm defaultFsm
        {
            get
            {
                return _fsms?.Length > 0? _fsms[0] : null;
            }
        }

        public Fsm GetFsm(string name)
        {
            if (_fsms == null || string.IsNullOrEmpty(name))
                return null;

            foreach (var fsm in _fsms)
            {
                if (fsm.name == name)
                {
                    return fsm;
                }
            }

            return null;
        }

        private void InitWithConfig(ConfigFsmController cfg)
        {
            _config = cfg;
            _variableSet = ObjectPool.Instance.Fetch<VariableSet>();

            _config.InitDefaultParam(this);
            _triggers = ListComponent<ConfigParamTrigger>.Create();
            foreach (var item in _config.paramDict)
            {
                if (item.Value is ConfigParamTrigger trigger)
                {
                    _triggers.Add(trigger);
                }
            }
            _fsms = new Fsm[_config.fsmCount];
            for (int i = 0; i < _config.fsmCount; i++)
            {
                var fsmCfg = _config.GetFsmConfig(i);
                _fsms[i] = CreateFsm(fsmCfg);
            }
        }

        protected virtual Fsm CreateFsm(ConfigFsm fsmCfg)
        {
            return Fsm.Create(this, fsmCfg);
        }

        public void Start()
        {
            for (int i = 0; i < _config.fsmCount; i++)
            {
                _fsms[i].Start();
            }
        }

        public void Update()
        {
            for (int i = 0; i < _fsms.Length; i++)
            {
                if (_fsms[i] == null) continue; //可能在其他状态中entity被销毁了
                _fsms[i].Update(GameTimerManager.Instance.GetDeltaTime() / 1000f);
            }

            for (int i = 0; i < _triggers.Count; i++)
            {
                _triggers[i].SetValue(this,false);
            }
        }

        public void Stop()
        {
        }

        #region IComponent

        public virtual void Init(ConfigFsmController cfg)
        {
            InitWithConfig(cfg);
            Start();
        }

        public virtual void Destroy()
        {
            Stop();
            if (_fsms != null)
            {
                for (int i = 0; i < _fsms.Length; i++)
                {
                    _fsms[i].Dispose();
                    _fsms[i] = null;
                }
            }

            if (_variableSet != null)
            {
                _variableSet.Dispose();
                _variableSet = null;
            }
            _triggers.Dispose();
            _triggers = null;
            _config = null;
        }

        #endregion

        #region Data

        public bool KeyExist(string key)
        {
            return _config.TryGetParam(key, out _);
        }

        public void SetData(string key, float val)
        {
            if (_config.TryGetParam(key, out var param))
            {
                param.SetValue(this, val);
            }
            else
            {
                Log.Error("FsmController SetData Can Not Find float Param: {0}", key);
            }
        }

        public void SetData(string key, int val)
        {
            if (_config.TryGetParam(key, out var param))
            {
                param.SetValue(this, val);
            }
            else
            {
                Log.Error("FsmController SetData Can Not Find int Param: {0}", key);
            }
        }

        public void SetData(string key, bool val)
        {
            if (_config.TryGetParam(key, out var param))
            {
                param.SetValue(this, val);
            }
            else
            {
                Log.Error("FsmController SetData Can Not Find bool Param: {0}", key);
            }
        }

        public float GetFloat(string key)
        {
            if (_config.TryGetParam(key, out var param))
            {
                return param.GetFloat(this);
            }
            else
            {
                Log.Error("FsmController GetFloat Can Not Find float Param: {0}", key);
                return default (float);
            }
        }

        public int GetInt(string key)
        {
            if (_config.TryGetParam(key, out var param))
            {
                return param.GetInt(this);
            }
            else
            {
                Log.Error("FsmController GetInt Can Not Find int Param: {0}", key);
                return default (int);
            }
        }

        public bool GetBool(string key)
        {
            if (_config.TryGetParam(key, out var param))
            {
                return param.GetBool(this);
            }
            else
            {
                Log.Error("FsmController GetBool Can Not Find bool Param: {0}", key);
                return default (bool);
            }
        }

        public bool IsExist(string key)
        {
            return _config.TryGetParam(key, out var param);
        }

        #endregion
    }
}