namespace TaoTie
{
    public enum TargetType
    {
        None = 0,
        /// <summary>
        /// 同阵营
        /// </summary>
        Alliance = 1,
        Enemy = 2,
        Self = 3,
        SelfCamp = 4,
        All = 5,
        AllExceptSelf = 6
    }
}