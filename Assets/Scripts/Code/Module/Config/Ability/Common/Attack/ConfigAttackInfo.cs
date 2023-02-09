namespace TaoTie
{
    public class ConfigAttackInfo
    {
        /// <summary>
        /// 攻击标识，其他地方可通过这个标记筛选过滤
        /// </summary>
        public string AttackTag;
        /// <summary>
        /// 衰减类型标记
        /// </summary>
        public string AttenuationTag;
        /// <summary>
        /// 衰减分组
        /// </summary>
        public string AttenuationGroup;
        public ConfigAttackProperty AttackProperty;
        public ConfigHitPattern HitPattern;
        /// <summary>
        /// 是否有击中头部额外效果
        /// </summary>
        public bool CanHitHead;
        public ConfigHitPattern HitHeadPattern;
        public bool ForceCameraShake;
        public ConfigCameraShake CameraShake;
        /// <summary>
        /// 子弹衰减
        /// </summary>
        public ConfigBulletWane BulletWane;
    }
}