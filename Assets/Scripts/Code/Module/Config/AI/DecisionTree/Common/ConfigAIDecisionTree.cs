using ProtoBuf;

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigAIDecisionTree
    {
        [ProtoMember(1)]
        public DecisionArchetype Type;
        [ProtoMember(2)]
        public DecisionNode Node;
        [ProtoMember(3)]
        public DecisionNode CombatNode;
    }
}