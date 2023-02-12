using System;
using System.Collections.Generic;

namespace TaoTie
{
    public class ConfigFsm
    {
        public ConfigFsm(){}
        public string name;
        public int layerIndex;
        public string entry;
        public Dictionary<string, ConfigFsmState> stateDict;
        public ConfigTransition[] anyStateTransitions;


#if UNITY_EDITOR
        public ConfigFsm(string name, int layerIdx)
        {
            this.name = name;
            this.layerIndex = layerIdx;
        }

        public void SetEntry(string entryName)
        {
            this.entry = entryName;
        }

        public void SetAnyStateTransitions(ConfigTransition[] transitions)
        {
            this.anyStateTransitions = transitions;
        }

        public void SetStates(List<ConfigFsmState> states)
        {
            this.stateDict = new Dictionary<string, ConfigFsmState>();
            foreach (var state in states)
            {
                this.stateDict.Add(state.name, state);
            }
        }
#endif

        public ConfigFsmState GetStateConfig(string stateName)
        {
            if (this.stateDict != null)
            {
                this.stateDict.TryGetValue(stateName, out var cfg);
                return cfg;
            }
            return null;
        }

        public bool CheckAnyTransition(Fsm fsm, out ConfigTransition transtion)
        {
            if (this.anyStateTransitions != null)
            {
                for (int i = 0; i < this.anyStateTransitions.Length; ++i)
                {
                    if (this.anyStateTransitions[i].IsMatch(fsm))
                    {
                        transtion = this.anyStateTransitions[i];
                        return true;
                    }
                }
            }
            transtion = null;
            return false;
        }
    }
}