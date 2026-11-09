using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 读状态字典（dynDictionary）里的某个 key。
    ///
    /// <b>一个类型同时覆盖原来 AbilitySpecials 与 ModifierSpecials 两条读通道</b>：
    /// 早先 <c>AbilityValue</c> 只能读施法者那层，<c>ModifierValue</c> 只能读修饰器那层，
    /// 同一个 key 在两层的语义差别（一份施法者共享 / 每目标每施加各一份）在配置里看不出来，
    /// 换一层就得换类型。现在由 <see cref="Target"/> 显式决定读哪层，与
    /// <see cref="SetSpecial"/> / <see cref="AddSpecial"/> 的 <c>Target</c> 完全对称 ——
    /// 「写到哪层」和「从哪层读回来」用同一个枚举、同一套解析规则。
    ///
    /// <b>默认 <see cref="SpecialTarget.Auto"/></b>：传入了 modifier 就读这个实例，
    /// modifier 为 null 时才读 ability。这样「按当前盾量造成伤害」「剩余充能决定冷却」
    /// 写在修饰器 Mixin 里不用填字段，写在 ability 级 Mixin / 延时 Action 里也自动落到 Ability。
    ///
    /// <b>Auto 不会「读不到就换层」</b>：修饰器存在但 key 没配（ModifierSpecials 漏写、
    /// 或者拼错）时只报一次错并返回 <see cref="Default"/> —— 静默回退去读施法者共享值
    /// 会读出一个看似合理、实则张冠李戴的数字，比读不到更难查。
    /// 真要读 Ability 就显式填 <see cref="SpecialTarget.Ability"/>。
    /// </summary>
    [ProtoContract]
    public partial class SpecialValue : BaseValue
    {
        [ProtoMember(1)][LabelText("*状态名")]
        [Tooltip("对应 ConfigAbilityModifier.ModifierSpecials 或 ConfigAbility.AbilitySpecials 里的 key，如 Shield / Charge")]
        public string Key;

        [ProtoMember(2)][LabelText("读哪一层")]
        [Tooltip("Auto（默认）= 有修饰器读修饰器，没有则读 AbilitySpecials；\n" +
                 "Modifier = 只读修饰器，没有修饰器就报错；\n" +
                 "Ability = 只读 AbilitySpecials。")]
        public SpecialTarget Target = SpecialTarget.Auto;

        [ProtoMember(3)][LabelText("读不到时返回")]
        public float Default;

        private bool warned;

        public override float Resolve(Entity entity, ActorAbility ability, ActorModifier modifier = null)
        {
            if (string.IsNullOrEmpty(Key))
            {
                Warn(ability, "Key 为空");
                return Default;
            }

            // 与 SetSpecial / AddSpecial 共用同一套层解析，避免「写的是这层、读的是那层」
            var context = TargetHelper.ResolveSpecialTarget(Target, ability, modifier, out var error);
            if (context == null)
            {
                Warn(ability, error);
                return Default;
            }

            if (context.TryGetSpecial(Key, out var value)) return value;

            Warn(ability, $"{Describe(context)} 上没有状态 {Key}（检查 ModifierSpecials / AbilitySpecials）");
            return Default;
        }

        private static string Describe(BaseActorActionContext context)
        {
            return context is ActorModifier m
                ? $"修饰器 {m.Config?.ModifierName}"
                : "AbilitySpecials";
        }

        private void Warn(ActorAbility ability, string reason)
        {
            if (warned) return;
            warned = true;
            Log.Error($"[SpecialValue] {reason}；Ability = {ability?.Config?.AbilityName}");
        }
    }
}
