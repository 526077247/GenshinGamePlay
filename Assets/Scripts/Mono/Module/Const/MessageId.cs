namespace TaoTie
{
    public static class MessageId
    {
        /// <summary> 游戏时间缩放改变 </summary>
        public const int TimeScaleChange = -1;
        
        /// <summary> 数值变化 </summary>
        public const int NumericChangeEvt = 1;
        /// <summary> 坐标变化 </summary>
        public const int ChangePositionEvt = 2;
        /// <summary> 方向变化 </summary>
        public const int ChangeRotationEvt = 3;
        /// <summary> 缩放变化 </summary>
        public const int ChangeScaleEvt = 4;
        /// <summary> 关卡事件，所有关卡监听的事件都要用这个Id发出 </summary>
        public const int SceneGroupEvent = 5;
        /// <summary> PoseChange </summary>
        public const int PoseChange = 6;
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
        ///// <summary> 唤醒移动 </summary>
        public const int ResumePlatformMove = 16;
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
        public const int CombatStateChange = 23;
        /// <summary> 模型数量改变 </summary>
        public const int OnHolderCountChange = 24;
        /// <summary> 触发执行Ability </summary>
        public const int ExecuteAbility = 25;
        /// <summary> 清理某攻击实例的重复命中记录 </summary>
        public const int ClearHitRecord = 26;
        /// <summary> 检查左右脚 </summary>
        public const int CheckLeftOrRight = 27;
        /// <summary> 检查镜头与面向夹角 </summary>
        public const int CheckAngleVF = 28;
        /// <summary>
        /// 控制状态变化（载荷 ControlStateEvent）。只在被剥夺的能力位掩码真的变化时广播一次，
        /// 订阅方（UI 头顶图标 / 模型层特效 / AI 决策）据此响应，不要轮询 ControlComponent。
        /// </summary>
        public const int ControlStateChanged = 29;
        /// <summary>
        /// 状态标记变化（载荷 EntityTagsEvent）。标记集合或层数变化时广播，
        /// 订阅方拿到变化后自己拉层数，不要轮询 ControlComponent。
        /// </summary>
        public const int EntityTagsChanged = 30;
        /// <summary>
        /// FSM 发生状态切换（无参）。每次 ChangeState 无条件广播，订阅方据此重新聚合
        /// 状态派生量（武器显隐等），不要每帧轮询。与 SetShowWeapon 的差量广播相互独立：
        /// 后者只表达"某个 FSM 的 ShowWeapon 值翻转了"，多 FSM 下不能当作聚合结果用。
        /// </summary>
        public const int FsmStateChanged = 31;
        /// <summary>
        /// 被选中性 / 强制目标变化（无参）。订阅方（选敌缓存）据此立即刷新，不要每帧轮询。
        /// 与 ControlStateChanged 分开：后者是"能力被剥夺"，表现层会当成被控。
        /// </summary>
        public const int SelectionChanged = 32;
        /// <summary>
        /// 身上「挂了哪些元素」发生变化（载荷 = 实体 Id）。
        /// <b>只在元素的有无变化时广播</b>；元素量涨落不广播（每次命中都发会刷屏），
        /// 需要元素量数值的订阅方自己轮询 ElementComponent.GetDurability。
        /// </summary>
        public const int ElementChanged = 33;
        /// <summary>
        /// 触发了一次元素反应（载荷 = <c>ElementReactionEvent</c>）。
        /// 元素组件在每次附着后自动结算反应，判定成功即刻广播；<b>反应效果不在这里做</b> ——
        /// 配置层挂 <c>ConfigElementReactionMixin</c> 的 Ability 收到后自己算伤害 / 增伤。
        /// 订阅键 = 受击方实体 Id。
        /// </summary>
        public const int ElementReaction = 34;
    }
}