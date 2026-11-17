using ProtoBuf;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif

namespace TaoTie
{
    [ProtoContract]
    public partial class TryDoSkill: ConfigAbilityAction
    {
        [ProtoMember(10, IsRequired = true)][LabelText("是否按技能槽位释放")]
        public bool IsSlot = false;
        [ProtoMember(11)][LabelText("技能配置表Id")][ShowIf("@!"+nameof(IsSlot))]
        public int ConfigId;
        [ProtoMember(12)][LabelText("槽位号")][ShowIf(nameof(IsSlot))][Range(1, GameConst.SkillSlotConst)]
        public int Slot;

        /// <summary>
        /// 缓冲窗口（毫秒，游戏时间）。请求当场被拒（吟唱中/后摇/被控/技能CD/FSM 不收）时，
        /// 把这次意图记下来，在窗口内每帧重试，一旦放行就自动补发。
        /// 0 = 不缓冲，等价于"试一次就算了"。
        ///
        /// 默认 220：一次按键到下一次可输入的间隔（后摇尾段 + 人手反应）通常 150~250ms，
        /// 取偏保守的一端，能吃掉"差一点点"的输入，又不会让玩家在半秒后看到自动施法。
        /// </summary>
        [ProtoMember(13)][LabelText("缓冲窗口(ms)")][MinValue(0)]
        [Tooltip("被拒后在该窗口内自动补发；0 = 不缓冲")]
        public int BufferMs = 220;
        protected override void Execute(Entity actionExecuter, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            var cc = target.GetComponent<SkillComponent>();
            if (cc == null) return;

            // BufferMs 来自本条 action 自己的配置：不经 mixin、也不经 ability —— 一个键派发多个 action 时
            // 每个 action 各有各的窗口，不会耦合成"一个键共用一个窗口"。
            // 0 = 试一次就算了；>0 = 被拒时把意图记到 TTL 到期，由 SkillComponent.Update 每帧补发。
            if (IsSlot)
            {
                cc.RequestSkillBySlot(Slot, BufferMs);
            }
            else
            {
                cc.RequestSkill(ConfigId, BufferMs);
            }
        }
    }
}