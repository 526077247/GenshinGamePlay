﻿using System;
using System.Diagnostics;
using UnityEngine;

namespace TaoTie
{
    public class FsmComponent: Component, IComponent<ConfigFsmController>,IUpdate
    {
        private Fsm[] fsms;
        private ConfigFsmController config;
        protected DynDictionary dynDictionary;
        private ListComponent<ConfigParamTrigger> triggers;
        
        public Fsm[] Fsms => fsms;
        public ConfigFsmController Config => config;
        public DynDictionary DynDictionary => dynDictionary;
        public Fsm DefaultFsm => fsms?.Length > 0 ? fsms[0] : null;

        public event Action<string> OnFsmTimelineTriggerEvt; 
        public Fsm GetFsm(string name)
        {
            if (fsms == null || string.IsNullOrEmpty(name))
                return null;

            foreach (var fsm in fsms)
            {
                if (fsm.Name == name)
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
            if (fsms == null) return;
            using (ListComponent<ConfigParamTrigger> triggerBefore = ListComponent<ConfigParamTrigger>.Create())
            {
                if (triggers != null)
                {
                    for (int i = 0; i < triggers.Count; i++)
                    {
                        if (triggers[i].GetBool(DynDictionary))
                        {
                            triggerBefore.Add(triggers[i]);
                        }
                    }
                }
                
                for (int i = 0; i < fsms.Length; i++)
                {
                    if (fsms[i] == null) continue; //可能在其他状态中entity被销毁了
                    fsms[i].Update(GameTimerManager.Instance.GetDeltaTime() / 1000f);
                }
                
                //只还原已经触发过的Trigger
                for (int i = 0; i < triggerBefore.Count; i++)
                {
                    triggerBefore[i].SetValue(DynDictionary, false);
                }
            }
        }
        

        #region IComponent

        public virtual void Init(ConfigFsmController cfg)
        {
            InitWithConfig(cfg);
            Start();
        }

        public virtual void Destroy()
        {
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
                if(param.GetFloat(DynDictionary) == val) return;
                param.SetValue(DynDictionary, val);
                if (param.NeedSyncAnimator)
                {
                    if(param.ParameterType == AnimatorFsmType.Float)
                        Messager.Instance.Broadcast(Id, MessageId.SetAnimDataFloat, param.Key, val);
                    else if(param.ParameterType == AnimatorFsmType.Int)
                        Messager.Instance.Broadcast(Id, MessageId.SetAnimDataInt, param.Key, (int)val);
                }
            }
            else
            {
                LogWarning($"FsmController SetData Can Not Find float Param: {key}");
            }
        }

        public void SetData(string key, int val)
        {
            if (config.TryGetParam(key, out var param))
            {
                if(param.GetInt(DynDictionary) == val) return;
                param.SetValue(DynDictionary, val);
                if (param.NeedSyncAnimator)
                {
                    if(param.ParameterType == AnimatorFsmType.Int)
                        Messager.Instance.Broadcast(Id, MessageId.SetAnimDataInt, param.Key, val);
                    else if(param.ParameterType == AnimatorFsmType.Float)
                        Messager.Instance.Broadcast(Id, MessageId.SetAnimDataFloat, param.Key, (float)val);
                    
                }
            }
            else
            {
                LogWarning($"FsmController SetData Can Not Find int Param: {key}");
            }
        }

        public void SetData(string key, bool val)
        {
            if (config.TryGetParam(key, out var param))
            {
                if(param.GetBool(DynDictionary) == val) return;
                param.SetValue(DynDictionary, val);
                if (param.NeedSyncAnimator)
                {
                    Messager.Instance.Broadcast(Id, MessageId.SetAnimDataBool, param.Key, val, 
                        param.ParameterType == AnimatorFsmType.Trigger);
                }
            }
            else
            {
                LogWarning($"FsmController SetData Can Not Find bool Param: {key}");
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
                Log.Error($"FsmController GetFloat Can Not Find float Param: {key}");
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
                Log.Error($"FsmController GetInt Can Not Find int Param: {key}");
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
                Log.Error($"FsmController GetBool Can Not Find bool Param: {key}");
                return default (bool);
            }
        }

        public bool IsExist(string key)
        {
            return config.TryGetParam(key, out var param);
        }

        [Conditional("ENABLE_FSM_WARNING")]
        private void LogWarning(string str)
        {
            Log.Warning($"Id:{Id}\r\n" + str);
        }
        #endregion

        #region Trigger

        public void OnFsmTimelineTrigger(string triggerId)
        {
            OnFsmTimelineTriggerEvt?.Invoke(triggerId);
        }

        #endregion
    }
}