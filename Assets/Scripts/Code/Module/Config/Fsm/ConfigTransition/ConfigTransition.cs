using System;
using Nino.Core;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigTransition
    {
        [NinoMember(1)][ReadOnly]
        public string FromState;
        [NinoMember(2)][ReadOnly]
        public string ToState;
        [NinoMember(3)]
        public float ToStateTime;
        [NinoMember(4)]
        public float FadeDuration = 0.5f;
        [NinoMember(5)]
        public bool CanTransitionToSelf;
        [NinoMember(6)]
        public TransitionInterruptionSource InteractionSource;
        [NinoMember(7)][ShowIf("@"+nameof(InteractionSource)+"!="+nameof(TransitionInterruptionSource)+"."+nameof(TransitionInterruptionSource.None))]
        public bool OrderedInteraction;
        [NinoMember(8)]
        public ConfigCondition[] Conditions;

        public bool IsMatch(Fsm fsm)
        {
            if (!this.CanTransitionToSelf)
            {
                if (fsm.CurrentStateName == this.ToState) return false;
            }
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
            info.InteractionSource = InteractionSource;
        }

        public static void ApplyDefault(Fsm fsm, ConfigFsmState toStateCfg, ref FsmTransitionInfo info)
        {
            info.TargetName = toStateCfg.Name;
            info.LayerIndex = fsm.LayerIndex;
            info.TargetTime = 0;
            info.FadeDuration = 0.25f;
            info.InteractionSource = TransitionInterruptionSource.None;
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