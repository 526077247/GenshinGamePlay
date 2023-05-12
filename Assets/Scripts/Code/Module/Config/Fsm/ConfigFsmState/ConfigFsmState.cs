using System;
using LitJson.Extensions;
using Nino.Serialization;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigFsmState
    {
        public string Name;
        public float StateDuration;
        public string MirrorParameter;
        public bool StateLoop;
        public ConfigFsmTimeline Timeline;
        public ConfigTransition[] Transitions;
        public StateData Data;
        [JsonIgnore]
        [NinoIgnore]
        public bool HasTimeline => this.Timeline?.Clips?.Length > 0;
        
        
        public ConfigFsmState(){}
        public ConfigFsmState(string name, float stateDuration, bool loop, string mirrorParam)
        {
            this.Name = name;
            this.MirrorParameter = mirrorParam;
            this.StateDuration = stateDuration;
            this.StateLoop = loop;
        }

        public bool CheckTransition(Fsm fsm, out ConfigTransition transtion)
        {
            if (this.Transitions != null)
            {
                for (int i = 0; i < this.Transitions.Length; i++)
                {
                    if (this.Transitions[i].IsMatch(fsm))
                    {
                        transtion = this.Transitions[i];
                        return true;
                    }
                }
            }

            transtion = null;
            return false;
        }
    }
}