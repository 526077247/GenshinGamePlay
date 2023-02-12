using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigAIBeta
    {
        [NinoMember(1)][LabelText("技能")]
        public ConfigAISkill[] Skills;
        [NinoMember(2)][LabelText("行为")][Tooltip("优先级从上到下")]
        public ConfigAITacticBaseSetting[] Tactics;
    }
}