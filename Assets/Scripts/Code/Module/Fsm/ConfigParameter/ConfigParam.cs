using System;
using UnityEngine;

namespace TaoTie
{
    public abstract class ConfigParam
    {
        public string key;
        public bool needSyncAnimator;
        public int keyHash = 0;
        

        public void SetValue(FsmComponent ctrl, float val)
        {
            
            ctrl.variableSet.Set(this.key, val);
            if (needSyncAnimator)
            {
                ctrl.animator.SetFloat(keyHash, val);
            }
        }

        public void SetValue(FsmComponent ctrl, int val)
        {
            
            ctrl.variableSet.Set(this.key, val);
            if (needSyncAnimator)
            {
                ctrl.animator.SetInteger(keyHash, val);
            }
        }

        public void SetValue(FsmComponent ctrl, bool val)
        {
            
            ctrl.variableSet.Set(this.key, val?1:0);
            if (needSyncAnimator)
            {
                ctrl.animator.SetBool(keyHash, val);
            }
        }

        public float GetFloat(FsmComponent ctrl)
        {
            return ctrl.variableSet.Get(this.key);
        }

        public int GetInt(FsmComponent ctrl)
        {
            return (int)ctrl.variableSet.Get(this.key);
        }

        public bool GetBool(FsmComponent ctrl)
        {
            return ctrl.variableSet.Get(this.key) != 0;
        }

        public abstract void SetDefaultValue(FsmComponent ctrl);
    }
    
    public abstract class ConfigParam<T> : ConfigParam
    {
        public T defaultValue;
        public ConfigParam(){}
        public ConfigParam(string key, T defaultVal, bool needSyncAnimator)
        {
            base.key = key;
            this.defaultValue = defaultVal;
            this.keyHash = Animator.StringToHash(this.key);
            this.needSyncAnimator = needSyncAnimator;
        }
    }
}