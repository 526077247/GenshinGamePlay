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
    /// 免疫标记：挂着这条 Mixin 期间，命中 <see cref="ImmuneTags"/> 的施加一律被拒绝。
    ///
    /// <b>为什么是 Mixin 而不是 modifier 上的字段</b>：
    /// 1. 免疫是「行为」（拒绝别人施加的东西），不是「身份」——
    ///    <see cref="ConfigAbilityModifier.Tags"/> 回答"我是什么"，它要被
    ///    <see cref="ControlComponent"/> 全量扫描、被准入判定直接读，所以必须是平铺字段；
    ///    免疫只在 <see cref="AbilityComponent.ApplyModifier"/> 的入口被点查一次
    ///    （查 <see cref="ControlComponent"/>），源永远只有 0~2 条，适合用带生命周期钩子的 Mixin 注册表；
    /// 2. 挂在 Mixin 上天然拿到 afterAdd / beforeRemove：注册与注销跟着宿主走，
    ///    ControlComponent 只持一张 0~2 项的小表，不必跟着 modifier 增删做全量重算；
    /// 3. 顺带就能在免疫成功时执行 <see cref="Actions"/>，不必为此另开一条消息通道。
    ///
    /// 挂到 <see cref="ConfigAbility.AbilityMixins"/> 上也生效 —— 心法因此可以配常驻免疫，
    /// 不必非得做成一条 Duration=-1 的常驻 modifier。
    ///
    /// <b>本 Mixin 不管理次数</b>：它只判断「免不免」和「免了跑什么」。
    /// 有限次数、扣减、耗尽移除都是配置层用通用件拼的 ——
    /// <c>Actions</c> 里配 <c>AddSpecial</c> 扣，另挂
    /// <c>ConfigDoActionOnSpecialsChangeMixin</c> 监听同一个状态归零后移除宿主。
    /// 不配扣减就是永久免疫。
    /// </summary>
    [ProtoContract]
    [LabelText("免疫标记")]
    public partial class ConfigImmuneMixin : ConfigAbilityMixin
    {
        [ProtoMember(1)][LabelText("*免疫标记")][EnumToggleButtons]
        [Tooltip("挂着期间，命中这些标记的施加会被拒绝：对应的 Modifier 实例根本不创建，" +
                 "因此不会打断施法、不会扣它附带的属性、不会占用层数池。\n" +
                 "• 精确免疫：勾选具体标记（眩晕 / 冰冻 / 灼烧 …）\n" +
                 "• 整段免疫：勾 AllControlKind 免疫全部控制身份，含 bit 8..15 预留位 —— " +
                 "以后新增控制标记会自动被覆盖，不用回来改配置。\n" +
                 "注意免疫只挡「新的」，不会清掉已经挂在身上的状态；要清请配 RemoveTagsAction。")]
        public GameplayTags ImmuneTags;

        [ProtoMember(3)][LabelText("免疫成功时Actions:初始目标为宿主")]
        [Tooltip("成功挡下一次施加时执行，初始目标是宿主自己。这里配什么就做什么 ——\n" +
                 "• 抵抗表现：AttachEffect / PlaySound\n" +
                 "• 扣次数：AddSpecial{ Key = 任意状态名, Value = -1 }\n" +
                 "• 反击：Damage / 施加 modifier（Targetting 指向攻击者）\n" +
                 "只在本条真的挡住时触发；免疫标记不匹配的空过不会触发。\n" +
                 "想「扣到 0 次就消失」，另挂 ConfigDoActionOnSpecialsChangeMixin 监听同一个 key 归零，\n" +
                 "在它的 Actions 里配 RemoveModifier 即可 —— 本 Mixin 不参与次数判断。")]
        public ConfigAbilityAction[] Actions;
    }
}
