using ProtoBuf;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigSkillInfo
    {
        [ProtoMember(3)][LabelText("当前角色唯一Id")]
        public int LocalId;
        [ProtoMember(1)][LabelText("配置表Id")]
        public int ConfigId;
        [ProtoMember(2)][LabelText("触发Fsm时的输入Id")]
        public int SkillID;
    }
}