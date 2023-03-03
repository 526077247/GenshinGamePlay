using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
     public struct FsmTransitionInfo
     {
         public int targetHash;
         public int layerIndex;
         public float targetTime;
         public float fadeDuration;
     }

     public class Fsm : IDisposable
     {
         protected FsmComponent _component;
         private ConfigFsm _config;

         private readonly Dictionary<string, FsmState> _stateDict = new Dictionary<string, FsmState>();
         private FsmState _currentState;
         private float _stateTime;
         private float _stateNormalizedTime;
         private float _stateElapseTime;
         private float _statePassTime;
         private FsmTransitionInfo _transitionInfo;

         public string name => _config.name;
         public int layerIndex => _config.layerIndex;
         public FsmComponent Component => _component;
         public ConfigFsm config => _config;
         public FsmState currentState => _currentState;
         public string currentStateName => _currentState?.name;
         public float statePassTime => _statePassTime;
         public float stateTime => _stateTime;
         public float stateNormalizedTime => _stateNormalizedTime;

         public float stateElapseTime => _stateElapseTime;

         public delegate void StateChangedDelegate(string from, string to);
         public StateChangedDelegate onStateChanged;

         public static Fsm Create(FsmComponent ctrl, ConfigFsm cfg)
         {
             Fsm ret = ObjectPool.Instance.Fetch<Fsm>();
             ret.Init(ctrl, cfg);
             return ret;
         }

         protected virtual void Init(FsmComponent ctrl, ConfigFsm cfg)
         {
             _component = ctrl;
             _config = cfg;
         }

         public void Start()
         {
             ChangeState(_config.entry);
         }

         public void Update(float elapsetime)
         {
             if (_currentState != null)
             {
                 var stateCfg = _currentState.config;

                 _stateElapseTime = elapsetime;
                 _statePassTime += _stateElapseTime;
                 _stateTime += _stateElapseTime;
                 _stateNormalizedTime = _stateTime / stateCfg.stateDuration;
             }

             if (_config.CheckAnyTransition(this, out var transition))
             {
                 ChangeState(transition.toState, transition);
                 return;
             }

             if (_currentState != null)
             {
                 if (_currentState.config.CheckTransition(this, out transition))
                 {
                     ChangeState(transition.toState, transition);
                     return;
                 }

                 _currentState.OnUpdate();
             }
         }

         public void ChangeState(string name, ConfigTransition transition = null)
         {
             ConfigFsmState toCfg = null;
             if (!_stateDict.TryGetValue(name, out var toState))
             {
                 toCfg = _config.GetStateConfig(name);
                 if (toCfg == null)
                 {
                     Log.Error("ChangeState Missing State {0}", name);
                     return;
                 }

                 toState = FsmState.Create(this, toCfg);
                 _stateDict[name] = toState;
             }
             else
             {
                 toCfg = toState.config;
             }

             var fromState = _currentState;
             var fromCfg = fromState?.config;
             fromState?.OnExit();

             if (transition != null)
             {
                 transition.OnApply(this, fromCfg, toCfg, ref _transitionInfo);
             }
             else
             {
                 ConfigTransition.ApplyDefault(this, toCfg, ref _transitionInfo);
             }

             Messager.Instance.Broadcast(_component.Id, MessageId.CrossFadeInFixedTime, _transitionInfo.targetHash,
                 _transitionInfo.fadeDuration, _transitionInfo.layerIndex, _transitionInfo.targetTime);
             
             _statePassTime = 0;
             _stateTime = _transitionInfo.targetTime;
             _stateNormalizedTime = _transitionInfo.targetTime / toCfg.stateDuration;

             _currentState = toState;
             _currentState.OnEnter();
             transition?.OnPostApply(this);

             InvokeOnStateChanged(fromState, toState);
         }

         protected virtual void InvokeOnStateChanged(FsmState fromState, FsmState toState)
         {
             if (onStateChanged != null)
                 onStateChanged(fromState?.name, toState.name);
         }

         public ConfigFsmState GetStateConfig(string stateName)
         {
             return _config?.GetStateConfig(stateName);
         }

         #region IDisposable
         public virtual void Dispose()
         {
             _component = null;
             _config = null;
             _currentState = null;
             _stateDict.Clear();
         }
         #endregion
     }
}