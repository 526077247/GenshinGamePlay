using ProtoBuf;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigSkill
    {
        [ProtoMember(1)] [LabelText("默认技能")]
        public ConfigSkillInfo[] Skills;
    }
}