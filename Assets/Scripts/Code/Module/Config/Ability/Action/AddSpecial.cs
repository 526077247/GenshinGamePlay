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
    /// 给状态字典里的某个 key <b>加上</b>一个增量（可为负）。
    ///
    /// 与 <see cref="SetSpecial"/> 的分工：这个用于「扣盾 / 回盾 / 扣免疫次数」这类
    /// 需要读当前值的场景，那个用于「刷满 / 重置」。
    ///
    /// <b>写哪一层由 <see cref="Target"/> 决定</b>：默认 <see cref="SpecialTarget.Auto"/>
    /// 优先修饰器、没有才写 ability。修正常驻参数的写法是显式填
    /// <see cref="SpecialTarget.Ability"/>。
    ///
    /// <b>ClampMaxKey / ClampMinKey 不是可选项</b>：定时回盾、充能复位这类写法
    /// 必须带上限，否则 tick 会把值无限推高。上限本身也是同一个上下文里的 key，
    /// 所以「盾上限」和「当前盾」天然同层。
    /// </summary>
    [ProtoContract]
    public partial class AddSpecial : ConfigAbilityAction
    {
        [ProtoMember(10)][UnityEngine.Tooltip("状态名，如 Shield / Charge。拼错不报错，等于加了个没人读的值")]
        public string Key;

        [ProtoMember(11, IsRequired = true)][UnityEngine.Tooltip("增量，可为负（扣状态）。1 层消耗写 -1，每次回 10 点写 10")]
        public BaseValue Value = new ZeroValue();

        [ProtoMember(12)][UnityEngine.Tooltip("可选：写到这个 key 的当前值为上限。回盾/充能类必须填，否则会无限涨")]
        public string ClampMaxKey;

        [ProtoMember(13)][UnityEngine.Tooltip("可选：写到这个 key 的当前值为下限。消耗类一般填 0 或留空")]
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

            var owner = context.Parent != null ? context.Parent.GetParent<Entity>() : null;
            var delta = Value != null ? Value.Resolve(owner ?? target, ability, modifier) : 0f;
            if (delta == 0f) return;

            var next = context.GetSpecial(Key) + delta;

            if (!string.IsNullOrEmpty(ClampMaxKey))
            {
                var max = context.GetSpecial(ClampMaxKey);
                if (next > max) next = max;
            }

            if (!string.IsNullOrEmpty(ClampMinKey))
            {
                var min = context.GetSpecial(ClampMinKey);
                if (next < min) next = min;
            }

            // 同 SetSpecial：Set 之后可能触发监听方移除宿主，之后不得再访问成员
            context.SetSpecial(Key, next);
        }

        private void Warn(ActorAbility ability, string reason)
        {
            if (warned) return;
            warned = true;
            Log.Error($"[AddSpecial] {reason}；Ability = {ability?.Config?.AbilityName}");
        }
    }
}
