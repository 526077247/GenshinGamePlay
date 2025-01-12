using Nino.Core;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoType(false)]
    public partial class DecisionActionNode: DecisionNode
    {
        [NinoMember(10)][LabelText("行动类型")]
        public ActDecision Act;
        [NinoMember(11)][LabelText("移动类型")]
        public MoveDecision Move;
        [NinoMember(12)][LabelText("行动结果")]
        public AITactic Tactic;
    }
}