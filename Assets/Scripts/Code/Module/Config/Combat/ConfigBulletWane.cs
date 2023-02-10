using Nino.Serialization;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 子弹衰减
    /// </summary>
    [NinoSerialize]
    public partial class ConfigBulletWane
    {
        [NinoMember(1)][Tooltip("表示从Bullet创生之后这么久，开始套用衰减规则")]
        public float WaneDelayRawNum;
        [NinoMember(2)][Tooltip("伤害衰减周期，每过这么久，进行一次伤害衰减")]
        public float DamageWaneIntervalRawNum;
        [NinoMember(3)][Tooltip("每次进行伤害衰减时，按照DamageWaneRatio的比例，在damagePercentage上进行【扣除】，最多扣除到0")]
        public float DamageWaneRatioRawNum;
        [NinoMember(4)][Tooltip("每次进行伤害衰减时，最低比例")]
        public float DamageWaneMinRatioRawNum;
        [NinoMember(5)][Tooltip("元素衰减周期，每过这么久，进行一次元素衰减")]
        public float ElementDurabilityWaneIntervalRawNum;
        [NinoMember(6)][Tooltip("每次进行元素衰减时衰减比例")]
        public float ElementDurabilityWaneRatioRawNum;
        [NinoMember(7)][Tooltip("衰减比例最低值")]
        public float ElementDurabilityWaneMinRatioRawNum;
        [NinoMember(8)][Tooltip("受击等级衰减周期，每过这么久，进行一次受击等级衰减，每次将当前受击等级往下降一级：Air→Heavy→Light→Shake→Mute")]
        public float HitLevelWaneIntervalRawNum;
        [NinoMember(9)][Tooltip("受击等级最少衰减为这个等级。如果原本就不高于这个等级，则不予衰减。")]
        public HitLevel HitLevelWaneMin; // 0x30
    }
}