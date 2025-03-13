namespace TaoTie
{
    public class TimerType
    {
        /// <summary> 数值更新 </summary>
        public const int NumericUpdate = 1000;
        /// <summary> Mixin更新 </summary>
        public const int TickMixin = 1001;
        /// <summary> Modifier过期 </summary>
        public const int ModifierExpired = 1002;
        /// <summary> 延迟销毁Entity </summary>
        public const int DelayDestroyEntity = 1003;
        /// <summary> 组件Update </summary>
        public const int ComponentUpdate = 1004;
        /// <summary> AttachToNormalizedTimeMixinUpdate </summary>
        public const int AttachToNormalizedTimeMixinUpdate = 1005;
        /// <summary> 游戏时间计时 </summary>
        public const int GameTimeEventTrigger = 1006;
    }
}