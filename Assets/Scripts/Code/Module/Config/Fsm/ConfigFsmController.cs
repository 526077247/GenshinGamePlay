using System;
using System.Collections.Generic;
using Nino.Core;

namespace TaoTie
{
    [NinoType(false)]
    public sealed partial class ConfigFsmController
    {
        [NinoMember(1)]
        public Dictionary<string, ConfigParam> ParamDict = new Dictionary<string, ConfigParam>();
        [NinoMember(2)]
        public ConfigFsm[] FsmConfigs;

        public int FsmCount => this.FsmConfigs.Length;

        public bool TryGetParam(string key, out ConfigParam param)
        {
            param = null;
            if (ParamDict == null) return false;
            return this.ParamDict.TryGetValue(key, out param);
        }

        public ConfigFsm GetFsmConfig(int idx)
        {
            if (idx >= 0 && idx < this.FsmConfigs.Length)
            {
                return this.FsmConfigs[idx];
            }
            return null;
        }

        public void InitDefaultParam(FsmComponent ctrl)
        {
            if (this.ParamDict != null)
            {
                foreach (var item in this.ParamDict)
                {
                    item.Value.SetDefaultValue(ctrl.DynDictionary);
                }
            }
        }
    }
}