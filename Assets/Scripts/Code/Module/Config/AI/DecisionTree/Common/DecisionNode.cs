using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    public abstract partial class DecisionNode
    {
        [NinoMember(1)]
        public bool Enable = true;
        [SerializeField] [LabelText("策划备注")][NinoMember(2)]
        private string Remark;
    }
}