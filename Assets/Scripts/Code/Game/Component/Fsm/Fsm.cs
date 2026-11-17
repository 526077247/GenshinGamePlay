using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// FSM 参数的"干跑"覆盖值：在 WouldAccept 期间顶替 FsmComponent 真实数据。
    /// 只读、不落库、不广播 animator —— 探针生命期内 GetBool 等返回这里的值。
    /// </summary>
    public readonly struct FsmProbe
    {
        public readonly string Key;
        public readonly int Value;

        public FsmProbe(string key, int value) { Key = key; Value = value; }
    }

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

        /// <summary>
        /// 正在切换的目标状态名（仅在 <see cref="ChangeState"/> 执行期间及其中触发的
        /// OnExit / clip.Break 内有效，方法返回后清空）。用于让「打断补执行」区分
        /// 是被连段内的下一个状态接管，还是被冲刺/受击/死亡硬取消。
        /// </summary>
        public string PendingTargetName { get; private set; }
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
            currentState = null;
            preState = null;
            transitionInfo = default;
            OnStateChanged = null;
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

        /// <summary>
        /// 干跑：如果现在把 key=value 写进去，状态机会不会接受。纯读：不写真实数据、不广播、不切状态。
        /// </summary>
        public bool WouldAccept(string key, int value) => WouldAccept(new FsmProbe(key, value));
        public bool WouldAccept(FsmProbe probe) => WouldAccept(probe, default, out _);

        /// <summary>
        /// 两个参数一起干跑（技能请求就是 UseSkill + SkillId 两个键）。
        /// 求值顺序镜像 Update：AnyState → 当前状态 → 过渡中的 InteractionSource 分支，
        /// 每处都是"第一条命中的转移说了算"。命中的那条转移必须真的依赖探针键才算"技能可放"。
        /// </summary>
        public bool WouldAccept(FsmProbe a, FsmProbe b) => WouldAccept(a, b, out _);

        private bool WouldAccept(FsmProbe a, FsmProbe b, out string detail)
        {
            detail = null;
            if (component == null || config == null || currentState == null)
            {
                detail = "no-fsm(component/config/currentState 为空)";
                return false;
            }

            component.BeginProbe(a, b);
            try
            {
                if (config.AnyStateTransitions != null)
                {
                    for (int i = 0; i < config.AnyStateTransitions.Length; i++)
                    {
                        var t = config.AnyStateTransitions[i];
                        if (!t.IsMatch(this)) continue;
                        if (t.UsesKey(a.Key, b.Key))
                        {
                            detail = $"AnyState[{i}]->{t.ToState} 依赖探针键(可放)";
                            return true;
                        }
                        detail = $"AnyState[{i}]->{t.ToState} 命中但不依赖探针键(probe=({a.Key},{b.Key}))";
                        return false;
                    }
                }

                if (preState == null)
                {
                    bool ok = FirstTransitionAccepts(currentState, a, b, out bool matched, out string sub);
                    detail = ok
                        ? $"state={currentState.Name} 命中技能转移({sub})"
                        : $"state={currentState.Name} 无技能转移(matched={matched}; {sub})";
                    return ok;
                }

                switch (transitionInfo.InteractionSource)
                {
                    case TransitionInterruptionSource.Source:
                    {
                        bool ok = FirstTransitionAccepts(preState, a, b, out bool matched, out string sub);
                        detail = ok
                            ? $"过渡中(pre={preState.Name}) Source 分支命中技能转移({sub})"
                            : $"过渡中(pre={preState.Name}, cur={currentState.Name}) Source 分支无技能转移(matched={matched}; {sub})";
                        return ok;
                    }
                    case TransitionInterruptionSource.Destination:
                    {
                        bool ok = FirstTransitionAccepts(currentState, a, b, out bool matched, out string sub);
                        detail = ok
                            ? $"过渡中(cur={currentState.Name}) Destination 分支命中技能转移({sub})"
                            : $"过渡中(cur={currentState.Name}, pre={preState.Name}) Destination 分支无技能转移(matched={matched}; {sub})";
                        return ok;
                    }
                    case TransitionInterruptionSource.DestinationThenSource:
                    {
                        if (FirstTransitionAccepts(currentState, a, b, out bool m1, out string s1))
                        {
                            detail = $"过渡中(cur={currentState.Name}) DstThenSrc: 先查 dst 命中({s1})";
                            return true;
                        }
                        if (m1)
                        {
                            detail = $"过渡中(cur={currentState.Name}) DstThenSrc: dst 命中但非技能转移({s1}) → 冻结";
                            return false;
                        }
                        if (FirstTransitionAccepts(preState, a, b, out bool m2, out string s2))
                        {
                            detail = $"过渡中(pre={preState.Name}) DstThenSrc: 后查 src 命中({s2})";
                            return true;
                        }
                        detail = $"过渡中 DstThenSrc: cur={currentState.Name} matched={m1}({s1}); pre={preState.Name} matched={m2}({s2})";
                        return false;
                    }
                    case TransitionInterruptionSource.SourceThenDestination:
                    {
                        if (FirstTransitionAccepts(preState, a, b, out bool m1, out string s1))
                        {
                            detail = $"过渡中(pre={preState.Name}) SrcThenDst: 先查 src 命中({s1})";
                            return true;
                        }
                        if (m1)
                        {
                            detail = $"过渡中(pre={preState.Name}) SrcThenDst: src 命中但非技能转移({s1}) → 冻结";
                            return false;
                        }
                        if (FirstTransitionAccepts(currentState, a, b, out bool m2, out string s2))
                        {
                            detail = $"过渡中(cur={currentState.Name}) SrcThenDst: 后查 dst 命中({s2})";
                            return true;
                        }
                        detail = $"过渡中 SrcThenDst: pre={preState.Name} matched={m1}({s1}); cur={currentState.Name} matched={m2}({s2})";
                        return false;
                    }
                    default:
                        detail = $"过渡中(cur={currentState.Name}, pre={preState.Name}) InteractionSource=None → 冻结(该过渡不可被技能打断)";
                        return false;
                }
            }
            finally
            {
                component.EndProbe();
            }
        }

        /// <returns>(是否接受, 是否至少命中一条转移, 命中/未命中说明)</returns>
        private bool FirstTransitionAccepts(FsmState state, FsmProbe a, FsmProbe b, out bool matched, out string detail)
        {
            matched = false;
            detail = null;
            var arr = state?.Config?.Transitions;
            if (arr == null) { detail = "该状态无 Transitions"; return false; }
            for (int i = 0; i < arr.Length; i++)
            {
                if (!arr[i].IsMatch(this)) continue;
                matched = true;
                if (arr[i].UsesKey(a.Key, b.Key))
                {
                    detail = $"->{arr[i].ToState} 依赖探针键";
                    return true;
                }
                detail = $"->{arr[i].ToState} 命中但不依赖探针键";
                return false;
            }
            detail = "无满足条件的转移";
            return false;
        }

        public void ChangeState(string name, ConfigTransition transition = null)
        {
            ConfigFsmState toCfg = config.GetStateConfig(name);
            if (toCfg == null)
            {
                Log.Error("ChangeState Missing State {0}", name);
                return;
            }

            // 供 OnExit 期的 clip 补执行判定「这次是被谁打断的」：
            // 过渡期 preState 退出时 currentState 已是新状态，但 FadeDuration==0 时 OnExit 在
            // currentState = toState 之前调用，只看 CurrentState 会读到「正在退出的那个状态」。
            PendingTargetName = name;

            FsmState toState = FsmState.Create(this, toCfg);

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

            FsmState disposeFrom = null;
            FsmState disposePre = preState;
            preState = null;
            if (disposePre != null && ReferenceEquals(disposePre, fromState))
                disposePre = null;

            if (transitionInfo.FadeDuration > 0)
            {
                preState = fromState;
                if (preState != null)
                {
                    preState.StateExitTime = preState.StatePassTime + transitionInfo.FadeDuration;
                }
            }
            else
            {
                if (fromState != null)
                {
                    fromState.OnExit();
                    disposeFrom = fromState;
                }
            }
            currentState = toState;
            currentState.OnEnter();
            transition?.OnPostApply(this);

            InvokeOnStateChanged(fromState, toState);

            Messager.Instance.Broadcast(component.Id, MessageId.FsmStateChanged);

            disposeFrom?.Dispose();
            disposePre?.OnExit();
            disposePre?.Dispose();

            // 过渡期 preState 的 OnExit 可能在此之后才发生（等 FadeDuration 走完），
            // 那时要靠 currentState 判断打断来源，所以这里只清「正在切换」的临时标记。
            PendingTargetName = null;
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

            var oldPre = preState;
            var oldCur = currentState;
            preState = null;
            currentState = null;

            oldPre?.Dispose();
            if (oldCur != null && !ReferenceEquals(oldCur, oldPre))
                oldCur.Dispose();
            transitionInfo = default;
            OnStateChanged = null;

            component = null;
            config = null;
        }

        #endregion
    }
}