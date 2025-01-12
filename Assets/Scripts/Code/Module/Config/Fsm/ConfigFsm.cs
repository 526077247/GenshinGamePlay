using System;
using System.Collections.Generic;
using Nino.Core;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigFsm
    {
        [NinoMember(1)]
        public string Name;
        [NinoMember(2)]
        public int LayerIndex;
        [NinoMember(3)]
        public string Entry;
        [NinoMember(4)]
        public Dictionary<string, ConfigFsmState> StateDict = new();
        [NinoMember(5)]
        public ConfigTransition[] AnyStateTransitions;

        public ConfigFsmState GetStateConfig(string stateName)
        {
            if (this.StateDict != null)
            {
                this.StateDict.TryGetValue(stateName, out var cfg);
                return cfg;
            }
            return null;
        }

        public bool CheckAnyTransition(Fsm fsm, out ConfigTransition transition)
        {
            if (AnyStateTransitions != null)
            {
                for (int i = 0; i < AnyStateTransitions.Length; ++i)
                {
                    if (AnyStateTransitions[i].IsMatch(fsm))
                    {
                        transition = AnyStateTransitions[i];
                        return true;
                    }
                }
            }
            transition = null;
            return false;
        }
    }
}