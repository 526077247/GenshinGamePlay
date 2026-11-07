namespace TaoTie
{
    public static class GameConst
    {
        /// <summary>
        /// 3人称机位配置id
        /// </summary>
        public const int ThirdCameraConfigId = 1;
        /// <summary>
        /// 技能插槽个数
        /// </summary>
        public const int SkillSlotConst = 4;
        
        /// <summary>
        /// 吟唱时长在 ability 里的变量名。施法时用 <c>SetSpecials</c> 写进去，
        /// 由该 ability 的 <c>DelayDoAction.Delay</c>（AbilityValue）读走当倒计时。
        /// </summary>
        public const string CastTimeSpecial = "CastTime";
    }
}