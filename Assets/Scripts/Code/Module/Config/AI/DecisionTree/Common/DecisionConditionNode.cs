using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoSerialize]
    public partial class DecisionConditionNode: DecisionNode
    {
        [NinoMember(10)][ValueDropdown("@OdinDropdownHelper.GetAIDecisionInterface()")]
        public string Condition;
        [NinoMember(11)]
        public DecisionNode True;
        [NinoMember(12)]
        public DecisionNode False;
    }
}