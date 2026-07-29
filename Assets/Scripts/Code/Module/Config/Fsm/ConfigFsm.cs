using System;
using System.Collections.Generic;
using ProtoBuf;

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigFsm
    {
        [ProtoMember(1)]
        public string Name;
        [ProtoMember(2)]
        public int LayerIndex;
        [ProtoMember(3)]
        public string Entry;
        [ProtoMember(4, IsRequired = true)]
        public Dictionary<string, ConfigFsmState> StateDict = new Dictionary<string, ConfigFsmState>();
        [ProtoMember(5)]
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