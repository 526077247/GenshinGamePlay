using System;
using Nino.Serialization;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigTransition
    {
        public enum MirrorMode
        {
            Auto,
            DoNotMirror,
            MirrorOnly,
            Opposite,
            DoNotChagne,
            DoNotSet,
        }

        public string FromState;
        public string ToState;
        public float ToStateTime;
        public float FadeDuration;   // fixed time in second
        public MirrorMode Mode;
        public bool CanTransitionToSelf; // just work in any state transitions
        public ConfigCondition[] Conditions;
        
        public ConfigTransition(){}
        public ConfigTransition(string fromState, string toState, ConfigCondition[] conds, float fadeDur, float toStateTime, bool canTransitionToSelf = false)
        {
            this.FromState = fromState;
            this.ToState = toState;
            this.Conditions = conds;
            this.FadeDuration = fadeDur;
            this.ToStateTime = toStateTime;
            this.CanTransitionToSelf = canTransitionToSelf;
            this.Mode = MirrorMode.DoNotSet;
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

            float targetMirror = 0f;
            float targetTime = 0f;
            float fromMirror = 0;
            if (!string.IsNullOrEmpty(fromStateCfg.MirrorParameter))
            {
                fromMirror = fsm.Component.GetFloat(fromStateCfg.MirrorParameter);
            }
            switch (this.Mode)
            {
                case MirrorMode.Auto:
                    targetMirror = fromMirror;
                    targetTime = this.ToStateTime;
                    break;
                case MirrorMode.DoNotMirror:
                    targetMirror = 0f;
                    targetTime = ToStateTime;
                    break;
                case MirrorMode.MirrorOnly:
                    targetMirror = 1f;
                    targetTime = ToStateTime;
                    break;
                case MirrorMode.Opposite:
                    {
                        targetMirror = fromMirror * -1 + 1;
                        targetTime = ToStateTime;
                    }
                    break;
                case MirrorMode.DoNotChagne:
                    {
                        targetMirror = fromMirror;
                        targetTime = ToStateTime;
                    }
                    break;
                case MirrorMode.DoNotSet:
                    {
                        // Todo: DoNotSet Add target time
                        targetTime = ToStateTime;
                    }
                    break;
                default:
                    break;
            }

            //NLog.Info(LogConst.FSM, "Transition:{0}->{1} from:[[ {2} {3} {4}]] to:[[ {5} {6} ]] Mirror:{7}", _fromState, _toState, fsm.controller.GetFloat(fromStateCfg.mirrorParameter ?? "") > 0, fsm.animatorStateTime, fsm.animatorStateNormalizedTime, targetMirror > 0, targetTime, _mirrorMode);

            if (!string.IsNullOrEmpty(toStateCfg.MirrorParameter) && this.Mode != MirrorMode.DoNotSet)
            {
                fsm.Component.SetData(toStateCfg.MirrorParameter, targetMirror);
            }

            info.TargetName = toStateCfg.Name;
            info.TargetTime = targetTime;
            info.LayerIndex = fsm.LayerIndex;
            info.FadeDuration = this.FadeDuration;
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