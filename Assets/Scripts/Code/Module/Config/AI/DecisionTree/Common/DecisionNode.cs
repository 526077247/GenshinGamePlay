using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    public abstract partial class DecisionNode
    {
        [NinoMember(1)]
        public bool Enable;
        [LabelText("策划备注")][NinoMember(2)]
        public string Remark;
    }
}