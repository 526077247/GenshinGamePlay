using System;
using TaoTie.LitJson.Extensions;
using Nino.Core;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
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