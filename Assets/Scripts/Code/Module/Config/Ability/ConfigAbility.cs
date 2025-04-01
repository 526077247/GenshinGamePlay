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
        [NinoMember(2)][LabelText("Ability参数")]
        public Dictionary<string, float> AbilitySpecials = new Dictionary<string, float>();
        [NinoMember(3)]
        public ConfigAbilityMixin[] AbilityMixins;
        [NinoMember(4)]
        public ConfigAbilityModifier[] Modifiers;
    }
}