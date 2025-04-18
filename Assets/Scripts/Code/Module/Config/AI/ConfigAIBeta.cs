﻿using System.Collections.Generic;
using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigAIBeta
    {
        [NinoMember(1)][LabelText("启用")]
        public bool Enable = true;
        [NinoMember(2)][LabelText("AI类型")]
        public DecisionArchetype DecisionArchetype;

        [NinoMember(3)][LabelText("感知")][NotNull]
        public ConfigAISensing Sensing;
        [NinoMember(4)][LabelText("威胁")][NotNull]
        public ConfigAIThreatSetting Threat;
        
        [NinoMember(5)][BoxGroup("技能")][LabelText("*单位CD")][Tooltip("该单位每两次使用技能最少间隔时间")]
        public int GloabCD;
        [NinoMember(6)][BoxGroup("技能")][LabelText("单位CD组")]
        public Dictionary<int, int> SkillGroupCDConfigs = new Dictionary<int, int>();
        [NinoMember(7)][BoxGroup("技能")]
        public ConfigAISkill[] Skills;
        [NinoMember(8)][LabelText("防守范围")]
        public ConfigAIDefendArea DefendArea;
        [NinoMember(9)][LabelText("寻路数据")]
        public ConfigAIPathFindingSetting Path;
        
        
        [NinoMember(19)]
        public ConfigAIMove MoveSetting;
        [NinoMember(20)][BoxGroup("行为")]
        public ConfigAIFacingMoveSetting FacingMoveTactic;
        [NinoMember(21)][BoxGroup("行为")]
        public ConfigAIMeleeChargeSetting MeleeChargeTactic;
        [NinoMember(22)][BoxGroup("行为")]
        public ConfigAIFleeSetting FleeTactic;
        [NinoMember(23)][BoxGroup("行为")]
        public ConfigAIWanderSetting WanderTactic;
    }
}