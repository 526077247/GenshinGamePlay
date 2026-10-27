namespace TaoTie
{
    /// <summary>
    /// 击退来源方向。决定「冲量沿哪个方向施加」，与力度大小无关（力度看 HitImpulseX/Y）。
    ///
    /// 编号不使用连续重排：配置里存的是 int，删项只留空号，否则历史数据会整体错位。
    /// </summary>
    public enum RetreatType
    {
        /// <summary>沿「攻击者 → 受击者」的水平连线（默认；双方重合时由调用方兜底）</summary>
        ByAttacker = 0,
        /// <summary>沿命中方向（攻击盒法线 / 弹道方向）</summary>
        ByHitDirection = 1,
        /// <summary>
        /// 垂直于「攻击者 → 受击者」连线的切向（绕 Y 轴 +90°，即连线左手侧）。
        /// 用于横扫/浮空把目标甩向侧面。方向固定取一侧而不随机，
        /// 否则同一招打同一位置会左右乱飞。
        /// </summary>
        ByTangent = 2,
        /// <summary>
        /// 沿「攻击<b>起点</b> → 受击者」的水平连线。
        ///
        /// 与 <see cref="ByAttacker"/> 的区别只在子弹：子弹命中时攻击者可能被解析成子弹自己
        /// （TriggerAttackEvent 直接拿执行者当攻击者），此时用子弹位置算方向必然退化成零向量。
        /// 本类型改用 <see cref="AttackResult.OriginOwnerId"/>（射手）作为起点。
        /// 非子弹链路两者完全等价；取不到射手时也退化成 ByAttacker。
        /// </summary>
        ByOriginOwner = 3,
        /// <summary>命中方向反向（子弹把目标沿弹道推回去）</summary>
        ByHitDirectionInverse = 4
    }
}
