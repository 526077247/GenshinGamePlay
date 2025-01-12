using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
    public abstract partial class DecisionNode
    {
        [NinoMember(1)]
        public bool Enable = true;
#if UNITY_EDITOR
        [SerializeField] [LabelText("策划备注")]
        public string Remarks;
#endif
    }
}