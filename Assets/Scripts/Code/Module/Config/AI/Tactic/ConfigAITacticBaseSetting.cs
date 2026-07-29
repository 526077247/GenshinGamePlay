using System.Collections.Generic;
using ProtoBuf;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    public abstract partial class ConfigAITacticBaseSetting
    {
        [ProtoMember(1, IsRequired = true)][LabelText("启用")]
        public bool Enable = true;
        [ProtoMember(2)]
        public ConfigAITacticCondition Condition;
        [ProtoMember(3)][Tooltip("能使用的技能的配置表Id")]
        public int[] ConfigId;
        [ProtoMember(4, IsRequired = true)][Tooltip("重写每个Pose能使用的技能")]
        public Dictionary<int, int[]> OverwriteByPose = new Dictionary<int, int[]>();
    }
}
