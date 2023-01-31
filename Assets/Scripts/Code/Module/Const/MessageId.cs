namespace TaoTie
{
    public static class MessageId
    {
        /// <summary> 数值变化 </summary>
        public const string NumericChangeEvt = "NumericChangeEvt";
        /// <summary> 坐标变化 </summary>
        public const string ChangePositionEvt = "ChangePositionEvt";
        /// <summary> 方向变化 </summary>
        public const string ChangeRotationEvt = "ChangeRotationEvt";
        /// <summary> 当单位受伤 </summary>
        public const string AfterCombatUnitGetDamage = "AfterCombatUnitGetDamage";
        /// <summary> 当控制状态改变 </summary>
        public const string ActionControlActiveChange = "ActionControlActiveChange";
        /// <summary> 当开始移动 </summary>
        public const string MoveStart = "MoveStart";
        /// <summary> 当停止移动 </summary>
        public const string MoveStop = "MoveStop";
    }
}