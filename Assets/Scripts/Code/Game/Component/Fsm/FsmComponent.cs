using System;
using UnityEngine;

namespace TaoTie
{
    public class FsmComponent: Component, IComponent
    {
        protected Entity _entityLogic = null;

        private Fsm[] _fsms;
        private ConfigFsmController _config;
        protected Animator _animator;
        protected VariableSet _variableSet;

#if UNITY_EDITOR
        public Fsm[] Fsms => _fsms;
#endif

        public Animator animator => _animator;
        public VariableSet variableSet => _variableSet;

        public Fsm baseFsm
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

        public void InitWithConfig(ConfigFsmController cfg)
        {
            _config = cfg;
            _variableSet = ObjectPool.Instance.Fetch<VariableSet>();

            _config.InitDefaultParam(this);

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

        public void Update(float nowtime, float elapsetime)
        {
            for (int i = 0; i < _fsms.Length; i++)
            {
                if (_fsms[i] == null) continue; //可能在其他状态中entity被销毁了
                _fsms[i].Update(nowtime, elapsetime);
            }
        }

        public void Stop()
        {
        }

        public void SetWeight(int index, float weight)
        {
            if (animator != null)
            {
                animator.SetLayerWeight(index, weight);
            }
        }

        #region IComponent

        public Entity entityLogic => _entityLogic;

        public void Init()
        {

        }

        public void Destroy()
        {
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

            _animator = null;
            _config = null;
            _entityLogic = null;
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