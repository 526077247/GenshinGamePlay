using System.Collections.Generic;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigAIBeta
    {
        [NinoMember(1)][LabelText("AI类型")]
        public bool Enable;
        [NinoMember(2)][LabelText("AI类型")]
        public DecisionArchetype DecisionArchetype;

        [NinoMember(3)][LabelText("感知")]
        public ConfigAISensing Sensing;
        [NinoMember(4)][LabelText("威胁")]
        public ConfigAIThreatSetting Threat;
        
        [NinoMember(5)][BoxGroup("技能")]
        public int GloabCD;
        [NinoMember(6)][BoxGroup("技能")]
        public Dictionary<int, int> SkillGroupCDConfigs;
        [NinoMember(7)][BoxGroup("技能")]
        public ConfigAISkill[] Skills;
        [NinoMember(8)][BoxGroup("行为")][Tooltip("优先级从上到下")]
        public ConfigAITacticBaseSetting[] Tactics;


    }
}