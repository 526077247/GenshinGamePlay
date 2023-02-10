using System.Collections.Generic;
using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    public class ConfigAbility
    {
        [NinoMember(1)]
        public string AbilityName;
        [NinoMember(2)]
        public Dictionary<string, float> AbilitySpecials;
        [NinoMember(3)]
        public ConfigAbilityMixin[] AbilityMixins;
        [NinoMember(4)]
        public ConfigAbilityModifier[] Modifiers;
    }
}