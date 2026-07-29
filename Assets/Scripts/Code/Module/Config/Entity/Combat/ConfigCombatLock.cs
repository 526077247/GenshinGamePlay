using ProtoBuf;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigCombatLock
    {
        [ProtoMember(1, IsRequired = true)][MinValue(0.1f)][LabelText("覆盖范围")]
        public float OverrideRange = 3;
        [ProtoMember(2, IsRequired = true)][Range(1,180)][LabelText("覆盖面向角度范围")]
        public float AimAngle = 90;
    }
}