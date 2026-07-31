using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif

namespace TaoTie
{
    [ProtoContract]
    public partial class DecisionConditionNode: DecisionNode
    {
        [ProtoMember(10)]
#if UNITY_EDITOR
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetAIDecisionInterface)+"()")]
#endif
        public string Condition;
        [ProtoMember(11)][NotNull]
        public DecisionNode True;
        [ProtoMember(12)][NotNull]
        public DecisionNode False;
    }
}