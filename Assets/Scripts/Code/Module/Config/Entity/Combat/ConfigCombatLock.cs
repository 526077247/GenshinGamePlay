using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigCombatLock
    {
        [NinoMember(1)][Min(0.1f)][LabelText("覆盖范围")]
        public float OverrideRange = 3;
    }
}