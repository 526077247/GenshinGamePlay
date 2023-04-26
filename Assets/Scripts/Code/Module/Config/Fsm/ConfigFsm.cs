using System;
using System.Collections.Generic;
using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigFsm
    {
        public ConfigFsm(){}
        public string Name;
        public int LayerIndex;
        public string Entry;
        public Dictionary<string, ConfigFsmState> StateDict;
        public ConfigTransition[] AnyStateTransitions;


#if UNITY_EDITOR
        public ConfigFsm(string name, int layerIdx)
        {
            this.Name = name;
            this.LayerIndex = layerIdx;
        }

        public void SetEntry(string entryName)
        {
            this.Entry = entryName;
        }

        public void SetAnyStateTransitions(ConfigTransition[] transitions)
        {
            this.AnyStateTransitions = transitions;
        }

        public void SetStates(List<ConfigFsmState> states)
        {
            this.StateDict = new Dictionary<string, ConfigFsmState>();
            foreach (var state in states)
            {
                this.StateDict.Add(state.Name, state);
            }
        }
#endif

        public ConfigFsmState GetStateConfig(string stateName)
        {
            if (this.StateDict != null)
            {
                this.StateDict.TryGetValue(stateName, out var cfg);
                return cfg;
            }
            return null;
        }

        public bool CheckAnyTransition(Fsm fsm, out ConfigTransition transtion)
        {
            if (this.AnyStateTransitions != null)
            {
                for (int i = 0; i < this.AnyStateTransitions.Length; ++i)
                {
                    if (this.AnyStateTransitions[i].IsMatch(fsm))
                    {
                        transtion = this.AnyStateTransitions[i];
                        return true;
                    }
                }
            }
            transtion = null;
            return false;
        }
    }
}