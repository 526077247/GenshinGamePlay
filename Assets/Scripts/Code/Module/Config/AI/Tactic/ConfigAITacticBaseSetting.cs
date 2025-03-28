using System.Collections.Generic;
using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    public abstract partial class ConfigAITacticBaseSetting
    {
        [NinoMember(1)][LabelText("启用")]
        public bool Enable = true;
        [NinoMember(2)]
        public ConfigAITacticCondition Condition;
        [NinoMember(3)][Tooltip("能使用的技能")]
        public int[] SkillId;
        [NinoMember(4)][Tooltip("重写每个Pose能使用的技能")]
        public Dictionary<int, int[]> OverwriteByPose = new Dictionary<int, int[]>();
    }
}
