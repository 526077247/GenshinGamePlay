﻿using System;
using System.Collections.Generic;

namespace TaoTie
{
    public class FsmSystem : IManager
    {
        public static FsmSystem Instance { get; private set; }
        
        private Dictionary<Type, Type> configClipType;
        
        public void Init()
        {
            Instance = this;
            configClipType = AttributeManager.Instance.GetCreateTypeMap(TypeInfo<FsmClip>.Type);
        }
        public void Destroy()
        {
            Instance = null;
        }

        public FsmClip CreateFsmClip<T>(T config, FsmState state)where T :ConfigFsmClip
        {
            if(configClipType.TryGetValue(config.GetType(), out var runnerType))
            {
                var res = ObjectPool.Instance.Fetch(runnerType) as FsmClip;
                res.OnInit(state,config);
                return res;
            }
            Log.Error($"CreateFsmClip失败， 未实现{config.GetType()}对应的Clip");
            return null;
        }
    }
}