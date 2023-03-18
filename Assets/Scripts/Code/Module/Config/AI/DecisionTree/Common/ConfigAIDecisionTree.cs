using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigAIDecisionTree
    {
        [NinoMember(1)]
        public DecisionArchetype Type;
        [NinoMember(2)]
        public DecisionNode Node;
        [NinoMember(3)]
        public DecisionNode CombatNode;
    }
}