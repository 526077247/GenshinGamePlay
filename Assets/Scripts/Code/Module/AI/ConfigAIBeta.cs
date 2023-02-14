using System.Collections.Generic;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigAIBeta
    {
        [NinoMember(1)][BoxGroup("技能")]
        public int GloabCD;
        [NinoMember(2)][BoxGroup("技能")]
        public Dictionary<int, int> SkillGroupCDConfigs;
        [NinoMember(3)][BoxGroup("技能")]
        public ConfigAISkill[] Skills;
        [NinoMember(4)][BoxGroup("行为")][Tooltip("优先级从上到下")]
        public ConfigAITacticBaseSetting[] Tactics;

        
    }
}