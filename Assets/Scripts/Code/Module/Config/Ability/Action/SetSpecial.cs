using ProtoBuf;
#if UNITY_EDITOR
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif
#endif

namespace TaoTie
{
    /// <summary>
    /// 把状态字典里的某个 key <b>设为</b>指定值（不是加减）。
    ///
    /// <b>作用对象是「状态字典」本身，不是 Targetting 选出的实体。</b>
    /// 这是它与 <see cref="SetNumericValue"/> 的根本区别：后者写目标的
    /// <see cref="NumericComponent"/>（一个实体一份，多个来源共用一个池），
    /// 前者写 <see cref="BaseActorActionContext"/> 的 dynDictionary ——
    /// 修饰器级是「每个目标、每次施加各一份」，ability 级是「一个施法者一份」。
    ///
    /// <b>写哪一层由 <see cref="Target"/> 决定</b>：默认 <see cref="SpecialTarget.Auto"/>
    /// 优先修饰器、没有才写 ability，所以修饰器级配置不用填；常驻参数要写进 AbilitySpecials
    /// 时显式填 <see cref="SpecialTarget.Ability"/>。
    ///
    /// <b>配合 <see cref="BaseValue"/> 的意义</b>：<c>Value</c> 在宿主实体上求值，
    /// 所以「把当前盾设为上限」可以写成
    /// <c>SetSpecial{ Key = Shield, Value = SpecialValue{ Key = MaxShield } }</c>，
    /// 不管之前是多少都不会越界 —— 而加法要预先知道当前值，容易顶穿上限。
    /// </summary>
    [ProtoContract]
    public partial class SetSpecial : ConfigAbilityAction
    {
        [ProtoMember(10)][UnityEngine.Tooltip("状态名，如 Shield / Charge。拼错不报错，等于配了个没人读的值")]
        public string Key;

        [ProtoMember(11, IsRequired = true)]
        public BaseValue Value = new ZeroValue();

        [ProtoMember(12)][UnityEngine.Tooltip("可选：写到这个 key 的当前值为上限。留空 = 不封顶")]
        public string ClampMaxKey;

        [ProtoMember(13)][UnityEngine.Tooltip("可选：写到这个 key 的当前值为下限。留空 = 不封底")]
        public string ClampMinKey;

        [ProtoMember(14)][UnityEngine.Tooltip("写哪一层状态：\n" +
                                              "Auto（默认）= 有修饰器写修饰器，没有则写 Ability；\n" +
                                              "Modifier = 只写修饰器，没有修饰器就报错；\n" +
                                              "Ability = 只写 AbilitySpecials。")]
        public SpecialTarget Target = SpecialTarget.Auto;

        private bool warned;

        protected override void Execute(Entity actionExecuter, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            var context = TargetHelper.ResolveSpecialTarget(Target, ability, modifier, out var error);
            if (context == null)
            {
                Warn(ability, error);
                return;
            }

            if (string.IsNullOrEmpty(Key))
            {
                Warn(ability, "Key 为空，写入被忽略");
                return;
            }

            // 求值上下文用状态持有者的宿主实体，而不是 Targetting 选出的实体 ——
            // 状态属于上下文，它的「当前值」必须按宿主身上的属性来算。
            var owner = context.Parent != null ? context.Parent.GetParent<Entity>() : null;
            var value = Value != null ? Value.Resolve(owner ?? target, ability, modifier) : 0f;

            if (!string.IsNullOrEmpty(ClampMaxKey))
            {
                var max = context.GetSpecial(ClampMaxKey);
                if (value > max) value = max;
            }

            if (!string.IsNullOrEmpty(ClampMinKey))
            {
                var min = context.GetSpecial(ClampMinKey);
                if (value < min) value = min;
            }

            // Set 会触发 onSpecialChange，监听方可能在回调里移除宿主（帧末延迟）。
            // 所以这一句之后不得再访问 context / modifier / this。
            context.SetSpecial(Key, value);
        }

        private void Warn(ActorAbility ability, string reason)
        {
            if (warned) return;
            warned = true;
            Log.Error($"[SetSpecial] {reason}；Ability = {ability?.Config?.AbilityName}");
        }
    }
}
