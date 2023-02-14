using Nino.Serialization;
using Sirenix.OdinInspector;

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
        [NinoMember(8)][LabelText("该技能包含的State")]
        public string[] StateIds;
    }
}