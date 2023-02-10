using Nino.Serialization;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public class ConfigHitPattern
    {
        [NinoMember(1)][Tooltip("击中特效")]
        public string OnHitEffectName;
        [NinoMember(2)]
        public HitLevel HitLevel;
        [NinoMember(3)]
        public BaseValue HitImpulseX;
        [NinoMember(4)]
        public BaseValue HitImpulseY;
        [NinoMember(5)]
        public string HitImpulseType;
        [NinoMember(6)][Tooltip("冲刺中的击退数据")]
        public ConfigHitImpulse OverrideHitImpulse;
        [NinoMember(7)]
        public RetreatType RetreatType;
        [NinoMember(8)]
        public float HitHaltTimeRawNum;
        [NinoMember(9)]
        public float HitHaltTimeScaleRawNum;
        [NinoMember(10)]
        public bool CanBeDefenceHalt;
        [NinoMember(11)]
        public bool MuteHitText;
        [NinoMember(12)]
        public bool Recurring;
    }
}