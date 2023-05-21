using System;
using UnityEngine;

namespace TaoTie
{
    public class FsmComponent: Component, IComponent<ConfigFsmController>,IUpdate
    {
        private Fsm[] fsms;
        public Fsm[] Fsms => fsms;
        private ConfigFsmController config;
        public ConfigFsmController Config => config;
        protected DynDictionary dynDictionary;
        
        public DynDictionary DynDictionary => dynDictionary;
        private ListComponent<ConfigParamTrigger> triggers;
        public Fsm DefaultFsm
        {
            get
            {
                return fsms?.Length > 0? fsms[0] : null;
            }
        }

        public Fsm GetFsm(string name)
        {
            if (fsms == null || string.IsNullOrEmpty(name))
                return null;

            foreach (var fsm in fsms)
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
            if (cfg == null)
            {
                Log.Error("FSM为空");
                return;
            }
            config = cfg;
            dynDictionary = ObjectPool.Instance.Fetch<DynDictionary>();

            config.InitDefaultParam(this);
            triggers = ListComponent<ConfigParamTrigger>.Create();
            if (config.ParamDict != null)
            {
                foreach (var item in config.ParamDict)
                {
                    if (item.Value is ConfigParamTrigger trigger)
                    {
                        triggers.Add(trigger);
                    }
                }
            }

            fsms = new Fsm[config.FsmCount];
            for (int i = 0; i < config.FsmCount; i++)
            {
                var fsmCfg = config.GetFsmConfig(i);
                fsms[i] = CreateFsm(fsmCfg);
            }
        }

        protected virtual Fsm CreateFsm(ConfigFsm fsmCfg)
        {
            return Fsm.Create(this, fsmCfg);
        }

        public void Start()
        {
            for (int i = 0; i < config.FsmCount; i++)
            {
                fsms[i].Start();
            }
        }

        public void Update()
        {
            for (int i = 0; i < fsms.Length; i++)
            {
                if (fsms[i] == null) continue; //可能在其他状态中entity被销毁了
                fsms[i].Update(GameTimerManager.Instance.GetDeltaTime() / 1000f);
            }

            for (int i = 0; i < triggers.Count; i++)
            {
                triggers[i].SetValue(DynDictionary,false);
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
            if (fsms != null)
            {
                for (int i = 0; i < fsms.Length; i++)
                {
                    fsms[i].Dispose();
                    fsms[i] = null;
                }
            }

            if (dynDictionary != null)
            {
                dynDictionary.Dispose();
                dynDictionary = null;
            }
            triggers.Dispose();
            triggers = null;
            config = null;
        }

        #endregion

        #region Data

        public bool KeyExist(string key)
        {
            return config.TryGetParam(key, out _);
        }

        public void SetData(string key, float val)
        {
            if (config.TryGetParam(key, out var param))
            {
                param.SetValue(DynDictionary, val);
                if (param.NeedSyncAnimator)
                {
                    Messager.Instance.Broadcast(Id, MessageId.SetAnimDataFloat, param.Key, val);
                }
            }
            else
            {
                Log.Error("FsmController SetData Can Not Find float Param: {0}", key);
            }
        }

        public void SetData(string key, int val)
        {
            if (config.TryGetParam(key, out var param))
            {
                param.SetValue(DynDictionary, val);
                if (param.NeedSyncAnimator)
                {
                    Messager.Instance.Broadcast(Id, MessageId.SetAnimDataInt, param.Key, val);
                }
            }
            else
            {
                Log.Error("FsmController SetData Can Not Find int Param: {0}", key);
            }
        }

        public void SetData(string key, bool val)
        {
            if (config.TryGetParam(key, out var param))
            {
                param.SetValue(DynDictionary, val);
                if (param.NeedSyncAnimator)
                {
                    Messager.Instance.Broadcast(Id, MessageId.SetAnimDataBool, param.Key, val);
                }
            }
            else
            {
                Log.Error("FsmController SetData Can Not Find bool Param: {0}", key);
            }
        }

        public float GetFloat(string key)
        {
            if (config.TryGetParam(key, out var param))
            {
                return param.GetFloat(DynDictionary);
            }
            else
            {
                Log.Error("FsmController GetFloat Can Not Find float Param: {0}", key);
                return default (float);
            }
        }

        public int GetInt(string key)
        {
            if (config.TryGetParam(key, out var param))
            {
                return param.GetInt(DynDictionary);
            }
            else
            {
                Log.Error("FsmController GetInt Can Not Find int Param: {0}", key);
                return default (int);
            }
        }

        public bool GetBool(string key)
        {
            if (config.TryGetParam(key, out var param))
            {
                return param.GetBool(DynDictionary);
            }
            else
            {
                Log.Error("FsmController GetBool Can Not Find bool Param: {0}", key);
                return default (bool);
            }
        }

        public bool IsExist(string key)
        {
            return config.TryGetParam(key, out var param);
        }

        #endregion
    }
}