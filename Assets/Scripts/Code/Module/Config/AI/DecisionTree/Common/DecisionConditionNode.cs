using ProtoBuf;
using Sirenix.OdinInspector;

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