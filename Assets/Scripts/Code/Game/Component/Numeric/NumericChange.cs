namespace TaoTie
{
    /// <summary>
    /// 其他地方不要持有对NumericChange的引用
    /// </summary>
    public struct NumericChange
    {
        public Entity Parent;
        public int NumericType;

        /// <summary>
        /// 变更前的原始存储值。小数属性存的是 值*10000，整数属性就是值本身。
        /// 「成对加减」这类逻辑用 Raw，避免反复换算产生漂移。
        /// </summary>
        public long OldRaw;

        /// <summary>
        /// 变更后的原始存储值，语义同 <see cref="OldRaw"/>。
        /// </summary>
        public long NewRaw;

        /// <summary>
        /// 变更前的真实值（小数属性已按 10000 还原），和配置阈值比较用这个。
        /// </summary>
        public float OldReal;

        /// <summary>
        /// 变更后的真实值（小数属性已按 10000 还原）。
        /// </summary>
        public float NewReal;
    }
}