using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    public partial class DecisionConditionNode: DecisionNode
    {
        [NinoMember(10)]
        public DecisionNode True;
        [NinoMember(11)]
        public DecisionNode False;
    }
}