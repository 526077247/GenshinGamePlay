using Nino.Core;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigSkill
    {
        [NinoMember(1)] [LabelText("默认技能")]
        public int[] DefaultSkillIDs;
    }
}