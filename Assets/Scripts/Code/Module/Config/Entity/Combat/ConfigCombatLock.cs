using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigCombatLock
    {
        [NinoMember(1)][MinValue(0.1f)][LabelText("覆盖范围")]
        public float OverrideRange = 3;
        [NinoMember(2)][Range(1,180)][LabelText("覆盖面向角度范围")]
        public float AimAngle = 90;
    }
}