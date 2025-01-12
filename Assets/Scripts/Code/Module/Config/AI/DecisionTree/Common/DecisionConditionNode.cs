using Nino.Core;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoType(false)]
    public partial class DecisionConditionNode: DecisionNode
    {
        [NinoMember(10)]
#if UNITY_EDITOR
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetAIDecisionInterface)+"()")]
#endif
        public string Condition;
        [NinoMember(11)][NotNull]
        public DecisionNode True;
        [NinoMember(12)][NotNull]
        public DecisionNode False;
    }
}