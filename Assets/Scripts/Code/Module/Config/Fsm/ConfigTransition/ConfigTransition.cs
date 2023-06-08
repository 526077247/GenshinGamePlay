using System;
using Nino.Serialization;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigTransition
    {
        public string FromState;
        public string ToState;
        public float ToStateTime;
        public float FadeDuration;   // fixed time in second
        public bool CanTransitionToSelf; // just work in any state transitions
        public ConfigCondition[] Conditions;
        
        /// <summary>
        /// 反序列化用
        /// </summary>
        public ConfigTransition(){}
        public ConfigTransition(string fromState, string toState, ConfigCondition[] conds, float fadeDur, 
            float toStateTime, bool canTransitionToSelf = false)
        {
            this.FromState = fromState;
            this.ToState = toState;
            this.Conditions = conds;
            this.FadeDuration = fadeDur;
            this.ToStateTime = toStateTime;
            this.CanTransitionToSelf = canTransitionToSelf;
        }

        public bool IsMatch(Fsm fsm)
        {
            if (!this.CanTransitionToSelf && fsm.CurrentStateName == this.ToState)
                return false;

            if (this.Conditions != null)
            {
                for (int i = 0; i < this.Conditions.Length; i++)
                {
                    if (!this.Conditions[i].IsMatch(fsm))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public void OnApply(Fsm fsm, ConfigFsmState fromStateCfg, ConfigFsmState toStateCfg, ref FsmTransitionInfo info)
        {
            if (fromStateCfg == null || toStateCfg == null)
            {
                ApplyDefault(fsm, toStateCfg, ref info);
                return;
            }
            
            info.TargetName = toStateCfg.Name;
            info.TargetTime = ToStateTime;
            info.LayerIndex = fsm.LayerIndex;
            info.FadeDuration = FadeDuration;
        }

        public static void ApplyDefault(Fsm fsm, ConfigFsmState toStateCfg, ref FsmTransitionInfo info)
        {
            info.TargetName = toStateCfg.Name;
            info.LayerIndex = fsm.LayerIndex;
            info.TargetTime = 0;
            info.FadeDuration = 0.25f;
        }

        public void OnPostApply(Fsm fsm)
        {
            if (this.Conditions != null)
            {
                for (int i = 0; i < this.Conditions.Length; ++i)
                {
                    this.Conditions[i]?.OnTransitionApply(fsm);
                }
            }
        }
    }
}