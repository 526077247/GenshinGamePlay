using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 数值监听：订阅 <see cref="MessageId.NumericChangeEvt"/>，满足比较条件时执行 Actions。
    ///
    /// 两个必须守住的约定，否则要么收不到事件、要么误触发：
    /// 1. 事件只从**基础段**广播。派生段（如 ENBase 10121）写入后走五段重算，
    ///    最后由 <c>Insert(1012, result)</c> 广播，所以这里要把配置的 id 归一化到基础段；
    /// 2. 事件里 Raw 是落库的 long（小数属性是 值*10000），Real 是广播时已还原的真实值。
    ///    和配置阈值比较一律用 <c>OldReal/NewReal</c>，不要拿 Raw 直接比。
    /// </summary>
    public class DoActionOnNumericChangeMixin : AbilityMixin<ConfigDoActionOnNumericChangeMixin>
    {
        private NumericComponent numericComponent;
        private long numericComponentId;
        private int numericType;

        protected override void InitInternal(ActorAbility actorAbility, ActorModifier actorModifier, ConfigDoActionOnNumericChangeMixin config)
        {
            var owner = actorAbility.Parent.GetParent<Entity>();
            numericComponent = owner?.GetComponent<NumericComponent>();
            if (numericComponent == null)
            {
                Log.Error($"[{config.NumericType}] 数值监听挂载失败：{owner?.GetType().Name} 上没有 NumericComponent");
                return;
            }

            numericType = config.NumericType;
            if (numericType > NumericType.Max)
            {
                // 派生段不会自己广播：10121 的写入会在 Update() 里折算成 1012 再 Insert，
                // 因此监听 10121 永远收不到事件。这里兜底归一化，配置侧写派生段也能跑。
                int baseType = numericType / 10;
                Log.Warning($"数值监听配置了派生段 {numericType}，已归一化为基础段 {baseType}；建议直接填基础段");
                numericType = baseType;
            }

            // 组件 Id 在 Destroy 时可能被回收改写，先存下来，Dispose 时才能删对监听
            numericComponentId = numericComponent.Id;
            Messager.Instance.AddListener<NumericChange>(numericComponentId, MessageId.NumericChangeEvt, OnNumericChange);

            if (config.TriggerOnInit && CompareValue(numericComponent.GetAsFloat(numericType)))
            {
                DoActions();
            }
        }

        private void OnNumericChange(NumericChange evt)
        {
            // 订阅按组件 Id 存，实体销毁后可能还有一帧残留事件飘进来
            if (numericComponent == null || evt.NumericType != numericType) return;

            var oldValue = evt.OldReal;
            var newValue = evt.NewReal;

            bool matchOld = CompareValue(oldValue);
            bool matchNew = CompareValue(newValue);

            bool trigger;
            switch (Config.Mode)
            {
                case NumericTriggerMode.Level:
                    trigger = matchNew;
                    break;
                case NumericTriggerMode.FallingEdge:
                    // 「跨入」而非「本来就在里面」：已知旧值不满足、新值满足，只在这一刻成立
                    trigger = !matchOld && matchNew;
                    break;
                case NumericTriggerMode.RisingEdge:
                    trigger = matchOld && !matchNew;
                    break;
                case NumericTriggerMode.AnyChange:
                    trigger = true;
                    break;
                default:
                    trigger = false;
                    break;
            }

            if (trigger)
            {
                DoActions();
            }
        }
        
        private bool CompareValue(float value)
        {
            float threshold = Config.Value;
            switch (Config.Compare)
            {
                case CompareMode.Equal:
                    return Mathf.Approximately(value, threshold);
                case CompareMode.NotEqual:
                    return !Mathf.Approximately(value, threshold);
                case CompareMode.Greater:
                    return value > threshold;
                case CompareMode.Less:
                    return value < threshold;
                case CompareMode.LEqual:
                    return value <= threshold;
                case CompareMode.GEqual:
                    return value >= threshold;
                default:
                    return false;
            }
        }

        private void DoActions()
        {
            var actions = Config.Actions;
            if (actions == null || actions.Length == 0) return;

            var executer = GetActionExecuter();
            if (Config.Predicate != null && !Config.Predicate.Evaluate(executer, actorAbility, actorModifier, executer))
            {
                return;
            }

            for (int i = 0; i < actions.Length; i++)
            {
                actions[i].DoExecute(executer, actorAbility, actorModifier, executer);
            }
        }

        protected override void DisposeInternal()
        {
            if (numericComponentId != 0)
            {
                Messager.Instance.RemoveListener<NumericChange>(numericComponentId, MessageId.NumericChangeEvt, OnNumericChange);
                numericComponentId = 0;
            }
            numericComponent = null;
        }
    }
}
