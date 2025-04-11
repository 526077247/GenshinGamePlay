using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public struct FsmTransitionInfo
    {
        public string TargetName;
        public int LayerIndex;
        public float TargetTime;
        public float FadeDuration;
        public TransitionInterruptionSource InteractionSource;
    }

    public class Fsm : IDisposable
    {
        public bool IsDispose { get; private set; }
        protected FsmComponent component;
        private ConfigFsm config;
        
        private FsmState currentState;
        private FsmState preState;

        
        private FsmTransitionInfo transitionInfo;

        public string Name => config.Name;
        public int LayerIndex => config.LayerIndex;
        public FsmComponent Component => component;
        public ConfigFsm Config => config;
        public FsmState CurrentState => currentState;
        public FsmState PreState => preState;
        public string CurrentStateName => currentState?.Name;
        public float StatePassTime => currentState.StatePassTime;
        public float StateTime => currentState.StateTime;
        public float StateNormalizedTime => currentState.StateNormalizedTime;
        
        public float StateElapseTime => currentState.StateElapseTime;

        public delegate void StateChangedDelegate(string from, string to);

        public StateChangedDelegate OnStateChanged;

        public static Fsm Create(FsmComponent ctrl, ConfigFsm cfg)
        {
            Fsm ret = ObjectPool.Instance.Fetch<Fsm>();
            ret.Init(ctrl, cfg);
            return ret;
        }

        protected virtual void Init(FsmComponent ctrl, ConfigFsm cfg)
        {
            IsDispose = false;
            component = ctrl;
            config = cfg;
        }

        public void Start()
        {
            ChangeState(config.Entry);
        }

        public void Update(float elapsetime)
        {
            if (currentState != null)
            {
                var stateCfg = currentState.Config;

                currentState.StateElapseTime = elapsetime;
                currentState.StatePassTime += currentState.StateElapseTime;
                currentState.StateTime += currentState.StateElapseTime;
                currentState.StateNormalizedTime = currentState.StateTime / stateCfg.StateDuration;

                if (preState != null)
                {
                    preState.StateElapseTime = elapsetime;
                    preState.StatePassTime += preState.StateElapseTime;
                    preState.StateTime += preState.StateElapseTime;
                    preState.StateNormalizedTime = preState.StateTime / stateCfg.StateDuration;
                    if (preState.StateExitTime <= preState.StatePassTime)
                    {
                        preState.OnExit();
                        preState.Dispose();
                        preState = null;
                    }
                }
            }

            if (config.CheckAnyTransition(this, out var transition))
            {
                ChangeState(transition.ToState, transition);
                return;
            }

            if (currentState != null)
            {
                if (preState != null) //过渡中
                {
                    switch (transitionInfo.InteractionSource)
                    {
                        case TransitionInterruptionSource.Source:
                            if (preState.Config.CheckTransition(this, out transition))
                            {
                                ChangeState(transition.ToState, transition);
                                return;
                            }

                            break;
                        case TransitionInterruptionSource.Destination:
                            if (currentState.Config.CheckTransition(this, out transition))
                            {
                                ChangeState(transition.ToState, transition);
                                return;
                            }

                            break;
                        case TransitionInterruptionSource.DestinationThenSource:
                            if (currentState.Config.CheckTransition(this, out transition))
                            {
                                ChangeState(transition.ToState, transition);
                                return;
                            }

                            if (preState.Config.CheckTransition(this, out transition))
                            {
                                ChangeState(transition.ToState, transition);
                                return;
                            }

                            break;
                        case TransitionInterruptionSource.SourceThenDestination:
                            if (preState.Config.CheckTransition(this, out transition))
                            {
                                ChangeState(transition.ToState, transition);
                                return;
                            }

                            if (currentState.Config.CheckTransition(this, out transition))
                            {
                                ChangeState(transition.ToState, transition);
                                return;
                            }

                            break;
                        default:
                            break;
                    }
                }
                else //未过渡
                {
                    if (currentState.Config.CheckTransition(this, out transition))
                    {
                        ChangeState(transition.ToState, transition);
                        return;
                    }
                }
            }
            currentState?.OnUpdate();
            preState?.OnUpdate();
            
            // Debug 为啥死了没有过渡状态
            // if (Component.GetBool(FSMConst.Die))
            //     ;
        }

        public void ChangeState(string name, ConfigTransition transition = null)
        {
            ConfigFsmState toCfg = config.GetStateConfig(name);
            if (toCfg == null)
            {
                Log.Error("ChangeState Missing State {0}", name);
                return;
            }

            FsmState toState;
            if (preState!=null && preState.Name == transitionInfo.TargetName)
            {
                toState = preState;
            }
            else
            {
                toState = FsmState.Create(this, toCfg);
            }

            var fromState = currentState;
            var fromCfg = fromState?.Config;

            if (transition != null)
            {
                transition.OnApply(this, fromCfg, toCfg, ref transitionInfo);
            }
            else
            {
                ConfigTransition.ApplyDefault(this, toCfg, ref transitionInfo);
            }

            if (!(this is PoseFsm))
            {
                Messager.Instance.Broadcast(component.Id, MessageId.CrossFadeInFixedTime, transitionInfo.TargetName,
                    transitionInfo.FadeDuration, transitionInfo.LayerIndex, transitionInfo.TargetTime);
            }

            toState.StatePassTime = 0;
            toState.StateTime = transitionInfo.TargetTime;
            toState.StateNormalizedTime = transitionInfo.TargetTime / toCfg.StateDuration;

            if (transitionInfo.FadeDuration > 0)
            {
                preState = fromState;
                if (preState != null && transitionInfo.FadeDuration > 0)
                {
                    preState.StateExitTime = preState.StatePassTime + transitionInfo.FadeDuration;
                }
            }
            else
            {
                currentState.OnExit();
                currentState.Dispose();
                currentState = null;
            }
            currentState = toState;
            currentState.OnEnter();
            transition?.OnPostApply(this);

            InvokeOnStateChanged(fromState, toState);
        }

        protected virtual void InvokeOnStateChanged(FsmState fromState, FsmState toState)
        {
            if (OnStateChanged != null)
                OnStateChanged(fromState?.Name, toState.Name);
            if (!(this is PoseFsm))
            {
                if (fromState == null || fromState.CanMove != toState.CanMove)
                {
                    Messager.Instance.Broadcast(component.Id, MessageId.SetCanMove, toState.CanMove);
                }

                if (fromState == null || fromState.CanTurn != toState.CanTurn)
                {
                    Messager.Instance.Broadcast(component.Id, MessageId.SetCanTurn, toState.CanTurn);
                }

                if (fromState == null || fromState.ShowWeapon != toState.ShowWeapon)
                {
                    Messager.Instance.Broadcast(component.Id, MessageId.SetShowWeapon, toState.ShowWeapon);
                }

                if (fromState == null || fromState.UseRagDoll != toState.UseRagDoll)
                {
                    Messager.Instance.Broadcast(component.Id, MessageId.SetUseRagDoll, toState.UseRagDoll);
                }
            }
        }

        public ConfigFsmState GetStateConfig(string stateName)
        {
            return config?.GetStateConfig(stateName);
        }

        #region IDisposable

        public virtual void Dispose()
        {
            if (IsDispose) return;
            IsDispose = true;
            component = null;
            config = null;
            currentState = null;
        }

        #endregion
    }
}