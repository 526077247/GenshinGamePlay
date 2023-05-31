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
     }

     public class Fsm : IDisposable
     {
         public bool IsDispose { get; private set; }
         protected FsmComponent component;
         private ConfigFsm config;

         private readonly Dictionary<string, FsmState> stateDict = new Dictionary<string, FsmState>();
         private FsmState currentState;
         private float stateTime;
         private float stateNormalizedTime;
         private float stateElapseTime;
         private float statePassTime;
         private FsmTransitionInfo transitionInfo;

         public string Name => config.Name;
         public int LayerIndex => config.LayerIndex;
         public FsmComponent Component => component;
         public ConfigFsm Config => config;
         public FsmState CurrentState => currentState;
         public string CurrentStateName => currentState?.Name;
         public float StatePassTime => statePassTime;
         public float StateTime => stateTime;
         public float StateNormalizedTime => stateNormalizedTime;

         public float StateElapseTime => stateElapseTime;

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

                 stateElapseTime = elapsetime;
                 statePassTime += stateElapseTime;
                 stateTime += stateElapseTime;
                 stateNormalizedTime = stateTime / stateCfg.StateDuration;
             }

             if (config.CheckAnyTransition(this, out var transition))
             {
                 ChangeState(transition.ToState, transition);
                 return;
             }

             if (currentState != null)
             {
                 if (currentState.Config.CheckTransition(this, out transition))
                 {
                     ChangeState(transition.ToState, transition);
                     return;
                 }

                 currentState.OnUpdate();
             }
         }

         public void ChangeState(string name, ConfigTransition transition = null)
         {
             ConfigFsmState toCfg = null;
             if (!stateDict.TryGetValue(name, out var toState))
             {
                 toCfg = config.GetStateConfig(name);
                 if (toCfg == null)
                 {
                     Log.Error("ChangeState Missing State {0}", name);
                     return;
                 }

                 toState = FsmState.Create(this, toCfg);
                 stateDict[name] = toState;
             }
             else
             {
                 toCfg = toState.Config;
             }

             var fromState = currentState;
             var fromCfg = fromState?.Config;
             fromState?.OnExit();

             if (transition != null)
             {
                 transition.OnApply(this, fromCfg, toCfg, ref transitionInfo);
             }
             else
             {
                 ConfigTransition.ApplyDefault(this, toCfg, ref transitionInfo);
             }

             Messager.Instance.Broadcast(component.Id, MessageId.CrossFadeInFixedTime, transitionInfo.TargetName,
                 transitionInfo.FadeDuration, transitionInfo.LayerIndex, transitionInfo.TargetTime);
             
             statePassTime = 0;
             stateTime = transitionInfo.TargetTime;
             stateNormalizedTime = transitionInfo.TargetTime / toCfg.StateDuration;

             currentState = toState;
             currentState.OnEnter();
             transition?.OnPostApply(this);

             InvokeOnStateChanged(fromState, toState);
         }

         protected virtual void InvokeOnStateChanged(FsmState fromState, FsmState toState)
         {
             if (OnStateChanged != null)
                 OnStateChanged(fromState?.Name, toState.Name);
             if (fromState == null||fromState.CanMove != toState.CanMove)
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
         }

         public ConfigFsmState GetStateConfig(string stateName)
         {
             return config?.GetStateConfig(stateName);
         }

         #region IDisposable
         public virtual void Dispose()
         {
             if(IsDispose) return;
             IsDispose = true;
             component = null;
             config = null;
             currentState = null;
             stateDict.Clear();
         }
         #endregion
     }
}