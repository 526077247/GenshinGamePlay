using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 子弹衰减
    /// </summary>
    public class ConfigBulletWane
    {
        [Tooltip("表示从Bullet创生之后这么久，开始套用衰减规则")]
        public float WaneDelayRawNum;
        [Tooltip("伤害衰减周期，每过这么久，进行一次伤害衰减")]
        public float DamageWaneIntervalRawNum;
        [Tooltip("每次进行伤害衰减时，按照DamageWaneRatio的比例，在damagePercentage上进行【扣除】，最多扣除到0")]
        public float DamageWaneRatioRawNum;
        [Tooltip("每次进行伤害衰减时，最低比例")]
        public float DamageWaneMinRatioRawNum;
        [Tooltip("元素衰减周期，每过这么久，进行一次元素衰减")]
        public float ElementDurabilityWaneIntervalRawNum;
        [Tooltip("每次进行元素衰减时衰减比例")]
        public float ElementDurabilityWaneRatioRawNum;
        [Tooltip("衰减比例最低值")]
        public float ElementDurabilityWaneMinRatioRawNum;
        [Tooltip("受击等级衰减周期，每过这么久，进行一次受击等级衰减，每次将当前受击等级往下降一级：Air→Heavy→Light→Shake→Mute")]
        public float HitLevelWaneIntervalRawNum;
        [Tooltip("受击等级最少衰减为这个等级。如果原本就不高于这个等级，则不予衰减。")]
        public HitLevel HitLevelWaneMin; // 0x30
    }
}