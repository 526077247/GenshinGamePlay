using System.Collections.Generic;
using ProtoBuf;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigAIBeta
    {
        [ProtoMember(1, IsRequired = true)][LabelText("启用")]
        public bool Enable = true;
        [ProtoMember(2)][LabelText("AI类型")]
        public DecisionArchetype DecisionArchetype;

        [ProtoMember(3)][LabelText("感知")][NotNull]
        public ConfigAISensing Sensing;
        [ProtoMember(4)][LabelText("威胁")][NotNull]
        public ConfigAIThreatSetting Threat;
        
        [ProtoMember(5)][BoxGroup("技能")][LabelText("*单位CD")][Tooltip("该单位每两次使用技能最少间隔时间")]
        public int GloabCD;
        [ProtoMember(6, IsRequired = true)][BoxGroup("技能")][LabelText("单位CD组")]
        public Dictionary<int, int> SkillGroupCDConfigs = new Dictionary<int, int>();
        [ProtoMember(7)][BoxGroup("技能")]
        public ConfigAISkill[] Skills;
        [ProtoMember(8)][LabelText("防守范围")]
        public ConfigAIDefendArea DefendArea;
        [ProtoMember(9)][LabelText("寻路数据")]
        public ConfigAIPathFindingSetting Path;
        
        
        [ProtoMember(19)]
        public ConfigAIMove MoveSetting;
        [ProtoMember(20)][BoxGroup("行为")]
        public ConfigAIFacingMoveSetting FacingMoveTactic;
        [ProtoMember(21)][BoxGroup("行为")]
        public ConfigAIMeleeChargeSetting MeleeChargeTactic;
        [ProtoMember(22)][BoxGroup("行为")]
        public ConfigAIFleeSetting FleeTactic;
        [ProtoMember(23)][BoxGroup("行为")]
        public ConfigAIWanderSetting WanderTactic;
    }
}