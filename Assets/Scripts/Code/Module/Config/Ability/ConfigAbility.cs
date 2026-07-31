using System.Collections.Generic;
using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif
using UnityEngine;

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigAbility
    {
        [ProtoMember(1)][Tooltip("全局唯一")]
        public string AbilityName;
#if UNITY_EDITOR
        [LabelText("策划备注")]
        public string Remarks;
#endif
        [ProtoMember(2, IsRequired = true)][LabelText("Ability参数")]
        public Dictionary<string, float> AbilitySpecials = new Dictionary<string, float>();
        [ProtoMember(3)]
        public ConfigAbilityMixin[] AbilityMixins;
        [ProtoMember(4)]
        public ConfigAbilityModifier[] Modifiers;
        [ProtoMember(5)][LabelText("是否需要默认自动添加给Avatar")]
        public bool DefaultAvatar;
    }
}