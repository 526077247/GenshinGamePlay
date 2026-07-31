using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif

namespace TaoTie
{
    [ProtoContract]
    public partial class DecisionActionNode: DecisionNode
    {
        [ProtoMember(10)][LabelText("行动类型")]
        public ActDecision Act;
        [ProtoMember(11)][LabelText("移动类型")]
        public MoveDecision Move;
        [ProtoMember(12)][LabelText("行动结果")]
        public AITactic Tactic;
    }
}