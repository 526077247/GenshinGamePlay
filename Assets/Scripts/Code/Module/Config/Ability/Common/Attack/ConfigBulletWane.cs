namespace TaoTie
{
    /// <summary>
    /// 子弹衰减
    /// </summary>
    public class ConfigBulletWane
    {
        /// <summary>
        /// 表示从Bullet创生之后这么久，开始套用衰减规则
        /// </summary>
        public float WaneDelayRawNum;
        /// <summary>
        /// 伤害衰减周期，每过这么久，进行一次伤害衰减
        /// </summary>
        public float DamageWaneIntervalRawNum;
        /// <summary>
        /// 每次进行伤害衰减时，按照DamageWaneRatio的比例，在damagePercentage上进行【扣除】，最多扣除到0
        /// </summary>
        public float DamageWaneRatioRawNum;
        /// <summary>
        /// 每次进行伤害衰减时，最低比例
        /// </summary>
        public float DamageWaneMinRatioRawNum;
        /// <summary>
        /// 元素衰减周期，每过这么久，进行一次元素衰减
        /// </summary>
        public float ElementDurabilityWaneIntervalRawNum;
        /// <summary>
        /// 每次进行元素衰减时衰减比例
        /// </summary>
        public float ElementDurabilityWaneRatioRawNum;
        /// <summary>
        /// 衰减比例最低值
        /// </summary>
        public float ElementDurabilityWaneMinRatioRawNum;
        /// <summary>
        /// 受击等级衰减周期，每过这么久，进行一次受击等级衰减，每次将当前受击等级往下降一级：Air→Heavy→Light→Shake→Mute
        /// </summary>
        public float HitLevelWaneIntervalRawNum;
        /// <summary>
        /// 受击等级最少衰减为这个等级。如果原本就不高于这个等级，则不予衰减。
        /// </summary>
        public HitLevel HitLevelWaneMin; // 0x30
    }
}