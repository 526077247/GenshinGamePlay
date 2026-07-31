using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif

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