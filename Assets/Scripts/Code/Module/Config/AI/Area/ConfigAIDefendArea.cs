using ProtoBuf;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigAIDefendArea
    {
        [ProtoMember(1, IsRequired = true)]
        public bool Enable = true;
        [ProtoMember(2)][LabelText("防守距边界范围")]
        public float DefendRange; 
    }
}