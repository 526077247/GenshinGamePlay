using System;
using UnityEngine;

namespace TaoTie
{
    public abstract class ConfigSetParameter<T> : ConfigFsmAction
    {
        public string key;

        public T value;
        

        /// <summary>
        /// 获取设置参数的fsm Controller
        /// </summary>
        /// <param name="fsm"></param>
        /// <returns></returns>
        protected FsmComponent GetSetParameterTargetFsm(Fsm fsm)
        {
            return fsm.Component;
        }
    }

    
    public class ConfigSetParameterBool : ConfigSetParameter<bool>
    {
        public override void Excute(Fsm fsm)
        {
            FsmComponent component = GetSetParameterTargetFsm(fsm);
            component?.SetData(key, value);
        }
    }
    
    public class ConfigSetParameterInt : ConfigSetParameter<int>
    {
        public override void Excute(Fsm fsm)
        {
            FsmComponent component = GetSetParameterTargetFsm(fsm);
            component?.SetData(key, value);
        }
    }
    
    public class ConfigSetParameterFloat : ConfigSetParameter<float>
    {
        public override void Excute(Fsm fsm)
        {
            FsmComponent component = GetSetParameterTargetFsm(fsm);
            component?.SetData(key, value);
        }
    }
}