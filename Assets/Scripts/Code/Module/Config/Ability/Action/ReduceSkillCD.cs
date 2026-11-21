using ProtoBuf;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif

namespace TaoTie
{
    /// <summary>
    /// 减少技能剩余 CD。支持"指定技能"与"全部技能"两种寻址，固定毫秒与剩余比例两种口径。
    /// 默认目标是 <see cref="AbilityTargetting.Caster"/>（Ability 持有者）—— 减 CD 几乎总是减施法者自己的，
    /// 沿用基类默认的 Target 会在"命中目标触发"的链路里减到被打的怪物头上。
    /// </summary>
    [ProtoContract]
    [Tooltip("减少技能剩余CD（回拨LastSpellTime，不新增字段）")]
    public partial class ReduceSkillCD : ConfigAbilityAction
    {
        /// <summary>技能配置表 Id（<see cref="SkillConfig.Id"/>）。AllSkills=true 时忽略此项。</summary>
        [ProtoMember(11)][LabelText("技能配置表Id")][ShowIf("@!"+nameof(AllSkills))]
        public int ConfigId;

        [ProtoMember(12)][LabelText("固定减CD(毫秒)")]
        public int ReduceMs;

        /// <summary>按剩余 CD 的比例减，0~1。与 ReduceMs 可同时填，先减固定值再按比例。</summary>
        [ProtoMember(13)][LabelText("按比例减CD(0~1)")]
        public float ReduceRatio;

        /// <summary>为 true 时对全部已装配技能减 CD（ConfigId 可留 0）。</summary>
        [ProtoMember(14)][LabelText("对全部技能生效")]
        public bool AllSkills;

        public ReduceSkillCD()
        {
            Targetting = AbilityTargetting.Caster;
        }

        protected override void Execute(Entity actionExecuter, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            var cc = target?.GetComponent<SkillComponent>();
            if (cc == null) return;

            if (AllSkills)
            {
                if (ReduceMs > 0) cc.ReduceAllSkillCD(ReduceMs);
                // 全部技能按比例减：没有批量接口，逐个走单技能接口，语义与单技能一致
                if (ReduceRatio > 0f && cc.SkillInfoMap != null)
                {
                    foreach (var id in cc.SkillInfoMap.Keys) cc.ReduceSkillCDByRatio(id, ReduceRatio);
                }
                return;
            }

            if (ConfigId == 0) return;
            if (ReduceMs > 0) cc.ReduceSkillCD(ConfigId, ReduceMs);
            if (ReduceRatio > 0f) cc.ReduceSkillCDByRatio(ConfigId, ReduceRatio);
        }
    }
}
