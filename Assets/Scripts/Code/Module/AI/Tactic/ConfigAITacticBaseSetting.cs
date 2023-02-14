using System.Collections.Generic;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    public abstract partial class ConfigAITacticBaseSetting
    {
        [NinoMember(1)][LabelText("启用")]
        public bool Enable;
        [NinoMember(2)][Tooltip("处于这些Pose中时有效")]
        public int[] PoseId;
        [NinoMember(3)][Tooltip("能使用的技能")]
        public int[] SkillId;
        [NinoMember(4)][Tooltip("重写每个Pose能使用的技能")]
        public Dictionary<int, int[]> OverwriteByPose;
    }
}
