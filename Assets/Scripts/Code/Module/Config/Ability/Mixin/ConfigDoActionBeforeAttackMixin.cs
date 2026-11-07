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
    /// 攻击前
    /// </summary>
    [ProtoContract][LabelText("攻击前DoAction")]
    public partial class ConfigDoActionBeforeAttackMixin: ConfigAbilityMixin
    {
        [ProtoMember(1)][LabelText("Actions:初始Action目标(Target)为受击者")]
        public ConfigAbilityAction[] Actions;
        [ProtoMember(2)][LabelText("攻击标识过滤")]
        [Tooltip("只响应「本次这一击」的 AttackInfo.AttackTag 与之一致的攻击，留空表示不区分攻击来源。\n" +
                 "标识是字符串约定，攻击侧（ConfigAttackInfo.AttackTag）与这里必须完全一致 —— " +
                 "拼错不会报错也不会触发 —— 改标识时攻击侧与订阅侧必须一起改。")]
        [ValueDropdown("@" + nameof(OdinDropdownHelper) + "." + nameof(OdinDropdownHelper.GetAttackTags) + "()", AppendNextDrawer = true)]
        public string AttackTag;
    }
}