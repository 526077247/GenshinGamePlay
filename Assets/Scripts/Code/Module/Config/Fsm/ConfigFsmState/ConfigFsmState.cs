using System;
using LitJson.Extensions;
using Nino.Serialization;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigFsmState
    {
        [NinoMember(1)]
        public string Name;
        [NinoMember(2)]
        public float StateDuration;
        [NinoMember(3)]
        public bool StateLoop;
        [NinoMember(4)]
        public ConfigFsmTimeline Timeline;
        [NinoMember(5)]
        public ConfigTransition[] Transitions;
        [NinoMember(6)]
        public StateData Data;
        [JsonIgnore]
        [NinoIgnore]
        public bool HasTimeline => this.Timeline?.Clips?.Length > 0;
        

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