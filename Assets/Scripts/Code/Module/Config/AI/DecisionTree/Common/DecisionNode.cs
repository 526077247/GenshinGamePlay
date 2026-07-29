using ProtoBuf;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [ProtoContract]
    [ProtoInclude(100, typeof(DecisionActionNode))]
    [ProtoInclude(101, typeof(DecisionConditionNode))]
    public abstract partial class DecisionNode
    {
        [ProtoMember(1, IsRequired = true)]
        public bool Enable = true;
#if UNITY_EDITOR
        [SerializeField] [LabelText("策划备注")]
        public string Remarks;
#endif
    }
}