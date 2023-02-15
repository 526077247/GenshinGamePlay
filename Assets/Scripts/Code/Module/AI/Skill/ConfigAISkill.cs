using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigAISkill
    {
        [NinoMember(1)]
        public int SkillID;
        [NinoMember(2)]
        public ConfigAISkillType SkillType;

        [NinoMember(3)] [LabelText("释放时朝向目标？")]
        public bool FaceTarget;
        [NinoMember(4)] [LabelText("目标无效时是否可使用？")]
        public bool CanUseIfTargetInactive;
        [NinoMember(5)]
        public int CD;
        [NinoMember(6)][LabelText("忽略公共CD？")][BoxGroup("公共CD")]
        public bool IgnoreGCD;
        [NinoMember(7)][LabelText("公共CD组")][ShowIf("@!IgnoreGCD")][BoxGroup("公共CD")]
        public string PublicCDGroup;
        [NinoMember(8)][LabelText("公共CD是否需要进入冷却")][ShowIf("@!IgnoreGCD")][BoxGroup("公共CD")]
        public bool TriggerGCD;
        [NinoMember(9)] [LabelText("公共CD时长配置id")] [ShowIf("@!IgnoreGCD&&TriggerGCD")][BoxGroup("公共CD")]
        public int SkillGroupCDID;
        [NinoMember(10)][LabelText("该技能包含的State")][Tooltip("不处于这些状态中时算技能释放完成")]
        public string[] StateIds;
        [NinoMember(11)][LabelText("技能开始时就触发冷却")]
        public bool TriggerCDOnStart;
        [NinoMember(12)][LabelText("技能使用条件判断")]
        public ConfigAISkillCastCondition CastCondition;
    }
}