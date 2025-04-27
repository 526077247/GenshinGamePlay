using Nino.Core;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigSkillInfo
    {
        [NinoMember(3)][LabelText("当前角色唯一Id")]
        public int LocalId;
        [NinoMember(1)][LabelText("配置表Id")]
        public int ConfigId;
        [NinoMember(2)][LabelText("触发Fsm时的输入Id")]
        public int SkillID;
    }
}