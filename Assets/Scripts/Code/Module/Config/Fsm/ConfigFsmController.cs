using System;
using System.Collections.Generic;
using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    public sealed partial class ConfigFsmController
    {
        public Dictionary<string, ConfigParam> paramDict;
        public ConfigFsm[] fsmConfigs;

        public int fsmCount => this.fsmConfigs.Length;

        public bool TryGetParam(string key, out ConfigParam param)
        {
            return this.paramDict.TryGetValue(key, out param);
        }

        public ConfigFsm GetFsmConfig(int idx)
        {
            if (idx >= 0 && idx < this.fsmConfigs.Length)
            {
                return this.fsmConfigs[idx];
            }
            return null;
        }

        public void InitDefaultParam(FsmComponent ctrl)
        {
            if (this.paramDict != null)
            {
                foreach (var item in this.paramDict)
                {
                    item.Value.SetDefaultValue(ctrl.DynDictionary);
                }
            }
        }
    }
}