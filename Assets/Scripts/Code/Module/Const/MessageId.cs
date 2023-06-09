namespace TaoTie
{
    public static class MessageId
    {
        /// <summary> 数值变化 </summary>
        public const int NumericChangeEvt = 1;
        /// <summary> 坐标变化 </summary>
        public const int ChangePositionEvt = 2;
        /// <summary> 方向变化 </summary>
        public const int ChangeRotationEvt = 3;
        /// <summary> SceneGroupEvent </summary>
        public const int SceneGroupEvent = 4;
        /// <summary> PoseChange </summary>
        public const int PoseChange = 5;
        /// <summary> UpdateMotionFlag </summary>
        public const int UpdateMotionFlag = 6;
        /// <summary> UpdateTurnTargetPos </summary>
        public const int UpdateTurnTargetPos = 7;

        #region Animator

        public const int SetAnimDataFloat = 8;
        public const int SetAnimDataInt = 9;
        public const int SetAnimDataBool = 10;
        public const int CrossFadeInFixedTime = 11;

        #endregion

        /// <summary> 相机震动</summary>
        public const int ShakeCamera = 12;
        /// <summary> FSM修改是否可以移动</summary>
        public const int SetCanMove = 13;
        /// <summary> FSM修改是否可以旋转</summary>
        public const int SetCanTurn = 14;
        /// <summary> FSM设置武器显示隐藏 </summary>
        public const int SetShowWeapon = 15;
        /// <summary> 游戏时间计时 </summary>
        public const int GameTimeEventTrigger = 16;
        /// <summary> 伤害飘字 </summary>
        public const int ShowDamageText = 17;
        /// <summary> 交互面板 </summary>
        public const int ShowIntee = 18;
        /// <summary> 按键状态改变 </summary>
        public const int OnKeyInput = 19;
        /// <summary> 当被击杀 </summary>
        public const int OnBeKill = 20;
        /// <summary> 当击杀 </summary>
        public const int OnKill = 21;
        /// <summary> FSM设置使用RagDoll </summary>
        public const int SetUseRagDoll = 22;
        /// <summary> 战斗状态改变 </summary>
        public const int CombatStateChange = 6;
    }
}