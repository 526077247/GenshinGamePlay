using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoSerialize]
    public partial class DecisionConditionNode: DecisionNode
    {
        [NinoMember(10)]
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetAIDecisionInterface)+"()")]
        public string Condition;
        [NinoMember(11)][NotNull]
        public DecisionNode True;
        [NinoMember(12)][NotNull]
        public DecisionNode False;
    }
}