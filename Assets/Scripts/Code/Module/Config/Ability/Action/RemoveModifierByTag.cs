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
    /// 按<b>状态标记</b>清掉目标身上所有命中的 modifier（净化 / 驱散）。
    ///
    /// 与 <see cref="RemoveModifier"/> 是两件事，刻意分成两个 Action：
    /// 后者按 <c>(施法者 ability 名, ModifierName)</c> 定位「精确一条」，
    /// 这里按标记位定位「跨来源、可能多条」，底层走
    /// <see cref="ControlComponent.ClearTags"/> → <see cref="AbilityComponent.RemoveModifiersByTag"/>。
    ///
    /// <b>帧末延迟必须保留</b>：<see cref="RemoveModifier"/> / <see cref="RemoveAbility"/> 都等
    /// <see cref="UnityLifeTimeHelper.WaitUpdateFinish"/>，为的是别在别人遍历 modifier 链时改集合。
    /// <c>RemoveModifiersByTag</c> 内部虽有 snapshot，但 <c>BeforeRemove</c> 会触发
    /// DoActionBeforeRemoveMixin 的 Actions，改的仍是 <see cref="ControlComponent.Rebuild"/>
    /// 正在遍历的同一条链表；Tag 监听的 Actions 更是在 Rebuild 的广播栈里同步执行的，
    /// 同步清除会嵌套 Rebuild 并与外层的 OnMaskChanged(old, deny) 打架。
    /// 代价是净化晚一帧生效：同一条 Actions 里后续 Action 读 IsControlled 仍是旧值。
    ///
    /// 免疫（<see cref="ConfigImmuneMixin"/>）拒绝新的施加，本 Action 清掉已经挂着的状态，两者互补。
    /// </summary>
    [ProtoContract]
    public partial class RemoveModifierByTag : ConfigAbilityAction
    {
        [ProtoMember(10)][LabelText("*清除标记")][EnumToggleButtons]
        [Tooltip("清掉身上所有挂着这些标记的 modifier；命中任一位即算命中，不看来源、不限条数。\n" +
                 "• 精确：勾具体标记（眩晕 / 灼烧 …）\n" +
                 "• 整段：勾 AllControlKind 一次清掉全部控制身份，含 bit 8..15 预留位 —— " +
                 "以后新增控制标记自动被覆盖，不用回来改配置。\n" +
                 "不带标记的 modifier（纯属性 / 纯行为）不会被清掉。")]
        public GameplayTags Tags;

        /// <summary>
        /// 运行时校验只打一次。配置对象是全局共享的，而 Action 可能挂在 Tick Mixin 上高频执行，
        /// 每次都打日志会刷屏；非 ProtoMember 字段不落库，也不会污染 bytes。
        /// </summary>
        private bool warned;

        protected override void Execute(Entity actionExecuter, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            // Action 层目前没有加载期校验入口（ValidateModifier 只查 modifier，
            // ValidateMixins 够不到嵌在各类 Mixin 里的 Actions），所以漏填只能运行时吵一次。
            if (Tags == GameplayTags.None)
            {
                WarnOnce("没填清除标记(None)：这条 Action 什么也不会清");
                return;
            }

            GameplayTags unknown = Tags & ~(GameplayTagMeta.KnownMask | GameplayTagMeta.GroupMask);
            if (unknown != GameplayTags.None)
            {
                // 不拦：未知位本来就匹配不到任何 modifier（ValidateModifier 禁止它落库），
                // 继续清已命名位是安全的，拦掉反而让整条净化失效
                WarnOnce($"清除标记含未命名的位 {unknown}：预留位不会匹配到任何 modifier，" +
                         "请先在 GameplayTags 里给它起名");
            }

            // 没有 ControlComponent 就没有标记的唯一聚合者；这类实体上带 Tags 的施加在
            // ApplyModifier 入口已被整条拒绝，本来也没有可清的带标记 modifier。
            var control = target.GetComponent<ControlComponent>();
            if (control == null) return;

            ExecuteLater(control).Coroutine();
        }

        private void WarnOnce(string reason)
        {
            if (warned) return;
            warned = true;
            Log.Warning($"[GameplayTag] RemoveModifierByTag {reason}");
        }

        private async ETTask ExecuteLater(ControlComponent control)
        {
            await UnityLifeTimeHelper.WaitUpdateFinish();
            // 等一帧后宿主可能已销毁回池；ClearTags 内部也会判一次 IsDispose，这里是双保险
            if (!control.IsDispose) control.ClearTags(Tags);
        }
    }
}
