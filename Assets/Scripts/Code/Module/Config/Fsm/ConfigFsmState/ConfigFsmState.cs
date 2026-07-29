using System;
using TaoTie.LitJson.Extensions;
using ProtoBuf;
using UnityEngine;

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigFsmState
    {
        [ProtoMember(1)]
        public string Name;
        [ProtoMember(2)]
        public float StateDuration;
        [ProtoMember(3)]
        public bool StateLoop;
        [ProtoMember(4)]
        public ConfigFsmTimeline Timeline;
        [ProtoMember(5)]
        public ConfigTransition[] Transitions;
        [ProtoMember(6)]
        public StateData Data;
        [JsonIgnore]
        public bool HasTimeline => this.Timeline?.Clips?.Length > 0;
        

        public bool CheckTransition(Fsm fsm, out ConfigTransition transition)
        {
            if (this.Transitions != null)
            {
                for (int i = 0; i < this.Transitions.Length; i++)
                {
                    if (this.Transitions[i].IsMatch(fsm))
                    {
                        transition = this.Transitions[i];
                        return true;
                    }
                }
            }

            transition = null;
            return false;
        }
    }
}