using System;
using ProtoBuf;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigTransition
    {
        [ProtoMember(1)][ReadOnly][ShowIf("@"+nameof(FromState)+"!="+nameof(ToState))]
        public string FromState;
        [ProtoMember(2)][ReadOnly]
        public string ToState;
        [ProtoMember(3)]
        public float ToStateTime;
        [ProtoMember(4, IsRequired = true)]
        public float FadeDuration = 0.5f;
        [ProtoMember(5)]
        public bool CanTransitionToSelf;
        [ProtoMember(6)]
        public TransitionInterruptionSource InteractionSource;
        [ProtoMember(7)][ShowIf("@"+nameof(InteractionSource)+"!="+nameof(TransitionInterruptionSource)+"."+nameof(TransitionInterruptionSource.None))]
        public bool OrderedInteraction;
        [ProtoMember(8)]
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