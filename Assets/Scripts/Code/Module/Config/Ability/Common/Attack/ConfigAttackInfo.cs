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
        /// <summary>
        /// 伤害数据
        /// </summary>
        public ConfigAttackProperty AttackProperty;
        /// <summary>
        /// 打击数据
        /// </summary>
        public ConfigHitPattern HitPattern;
        /// <summary>
        /// 是否有击中头部额外效果
        /// </summary>
        public bool CanHitHead;
        /// <summary>
        /// 击中头部的效果
        /// </summary>
        public ConfigHitPattern HitHeadPattern;
        /// <summary>
        /// 强制抖动摄像机
        /// </summary>
        public bool ForceCameraShake;
        /// <summary>
        /// 抖动摄像机参数
        /// </summary>
        public ConfigCameraShake CameraShake;
        /// <summary>
        /// 子弹衰减
        /// </summary>
        public ConfigBulletWane BulletWane;
    }
}