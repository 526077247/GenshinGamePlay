using System;
using LitJson.Extensions;
using UnityEngine;

namespace TaoTie
{
    public class ConfigFsmState
    {
        public string name;
        public float stateDuration;
        public string mirrorParameter;
        public bool stateLoop;
        public int nameHash = 0;
        public ConfigFsmTimeline timeline;
        public ConfigTransition[] transitions;
        [JsonIgnore]
        public bool hasTimeline => this.timeline?.clips?.Length > 0;
        
        

        public ConfigFsmState(string name, float stateDuration, bool loop, string mirrorParam)
        {
            this.name = name;
            this.nameHash = Animator.StringToHash(this.name);
            this.mirrorParameter = mirrorParam;
            this.stateDuration = stateDuration;
            this.stateLoop = loop;
        }

        public bool CheckTransition(Fsm fsm, out ConfigTransition transtion)
        {
            if (this.transitions != null)
            {
                for (int i = 0; i < this.transitions.Length; i++)
                {
                    if (this.transitions[i].IsMatch(fsm))
                    {
                        transtion = this.transitions[i];
                        return true;
                    }
                }
            }

            transtion = null;
            return false;
        }
    }
}