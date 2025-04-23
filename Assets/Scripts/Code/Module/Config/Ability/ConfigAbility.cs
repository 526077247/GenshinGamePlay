using System.Collections.Generic;
using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigAbility
    {
        [NinoMember(1)][Tooltip("全局唯一")]
        public string AbilityName;
#if UNITY_EDITOR
        [LabelText("策划备注")]
        public string Remarks;
#endif
        [NinoMember(2)][LabelText("Ability参数")]
        public Dictionary<string, float> AbilitySpecials = new Dictionary<string, float>();
        [NinoMember(3)]
        public ConfigAbilityMixin[] AbilityMixins;
        [NinoMember(4)]
        public ConfigAbilityModifier[] Modifiers;
        [NinoMember(5)][LabelText("是否需要默认自动添加给Avatar")]
        public bool DefaultAvatar;
    }
}