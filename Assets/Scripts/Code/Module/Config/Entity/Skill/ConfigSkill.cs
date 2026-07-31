using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigSkill
    {
        [ProtoMember(1)] [LabelText("默认技能")]
        public ConfigSkillInfo[] Skills;
    }
}