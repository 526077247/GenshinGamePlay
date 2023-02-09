using System.Collections.Generic;

namespace TaoTie
{
    public class ConfigAbility
    {
        public string AbilityName;
        public Dictionary<string, float> AbilitySpecials;
        public ConfigAbilityMixin[] AbilityMixins;
        public ConfigAbilityModifier[] Modifiers;
    }
}