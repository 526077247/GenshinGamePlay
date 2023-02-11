using System;
using UnityEngine;

namespace TaoTie
{
    public class ConfigTransition
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

        public string fromState;
        public string toState;
        public float toStateTime;
        public float fadeDuration;   // fixed time in second
        public float fadePrependTime;
        public MirrorMode mirrorMode;
        public bool canTransitionToSelf; // just work in any state transitions
        public ConfigCondition[] conditions;
        

        public ConfigTransition(string fromState, string toState, ConfigCondition[] conds, float fadeDur, float toStateTime, bool canTransitionToSelf = false)
        {
            this.fromState = fromState;
            this.toState = toState;
            this.conditions = conds;
            this.fadeDuration = fadeDur;
            this.fadePrependTime = 0f;
            this.toStateTime = toStateTime;
            this.canTransitionToSelf = canTransitionToSelf;
            this.mirrorMode = MirrorMode.DoNotSet;
        }

        public bool IsMatch(Fsm fsm)
        {
            if (!this.canTransitionToSelf && fsm.currentStateName == this.toState)
                return false;

            if (this.conditions != null)
            {
                for (int i = 0; i < this.conditions.Length; i++)
                {
                    if (!this.conditions[i].IsMatch(fsm))
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
            if (!string.IsNullOrEmpty(fromStateCfg.mirrorParameter))
            {
                fromMirror = fsm.Component.GetFloat(fromStateCfg.mirrorParameter);
            }
            switch (this.mirrorMode)
            {
                case MirrorMode.Auto:
                    targetMirror = fromMirror;
                    targetTime = this.toStateTime;
                    break;
                case MirrorMode.DoNotMirror:
                    targetMirror = 0f;
                    targetTime = toStateTime;
                    break;
                case MirrorMode.MirrorOnly:
                    targetMirror = 1f;
                    targetTime = toStateTime;
                    break;
                case MirrorMode.Opposite:
                    {
                        targetMirror = fromMirror * -1 + 1;
                        targetTime = toStateTime;
                    }
                    break;
                case MirrorMode.DoNotChagne:
                    {
                        targetMirror = fromMirror;
                        targetTime = toStateTime;
                    }
                    break;
                case MirrorMode.DoNotSet:
                    {
                        // Todo: DoNotSet Add target time
                        targetTime = toStateTime;
                    }
                    break;
                default:
                    break;
            }

            //NLog.Info(LogConst.FSM, "Transition:{0}->{1} from:[[ {2} {3} {4}]] to:[[ {5} {6} ]] Mirror:{7}", _fromState, _toState, fsm.controller.GetFloat(fromStateCfg.mirrorParameter ?? "") > 0, fsm.animatorStateTime, fsm.animatorStateNormalizedTime, targetMirror > 0, targetTime, _mirrorMode);

            if (!string.IsNullOrEmpty(toStateCfg.mirrorParameter) && this.mirrorMode != MirrorMode.DoNotSet)
            {
                fsm.Component.SetData(toStateCfg.mirrorParameter, targetMirror);
            }

            info.targetHash = toStateCfg.nameHash;
            info.targetTime = targetTime;
            info.layerIndex = fsm.layerIndex;
            info.fadeDuration = this.fadeDuration;
        }

        public static void ApplyDefault(Fsm fsm, ConfigFsmState toStateCfg, ref FsmTransitionInfo info)
        {
            info.targetHash = toStateCfg.nameHash;
            info.layerIndex = fsm.layerIndex;
            info.targetTime = 0;
            info.fadeDuration = 0.25f;
        }

        public void OnPostApply(Fsm fsm)
        {
            if (this.conditions != null)
            {
                for (int i = 0; i < this.conditions.Length; ++i)
                {
                    this.conditions[i]?.OnTransitionApply(fsm);
                }
            }
        }
    }
}