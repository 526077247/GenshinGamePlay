using System.Collections.Generic;
using Nino.Core;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigAbility
    {
        [NinoMember(1)]
        public string AbilityName;
        [NinoMember(2)]
        public Dictionary<string, float> AbilitySpecials = new Dictionary<string, float>();
        [NinoMember(3)]
        public ConfigAbilityMixin[] AbilityMixins;
        [NinoMember(4)]
        public ConfigAbilityModifier[] Modifiers;
    }
}