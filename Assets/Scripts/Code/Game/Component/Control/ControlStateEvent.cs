namespace TaoTie
{
    /// <summary>
    /// 控制状态变化广播载荷（<see cref="MessageId.ControlStateChanged"/>）。
    ///
    /// 广播只在"掩码真的变了"时发一次，所以订阅方不需要轮询：
    /// UI 头顶图标、模型层眩晕特效、AI 决策都可以靠它驱动。
    ///
    /// <see cref="Level"/> 是当前生效控制里的最高等级，供表现层判断"新来的控制值不值得换表现"；
    /// 行为判定请用 <see cref="NewMask"/>，不要用等级做 if。
    /// </summary>
    public struct ControlStateEvent
    {
        public long Id;
        /// <summary>变化前的掩码</summary>
        public ControlFlag OldMask;
        /// <summary>变化后的掩码（None = 已完全解除控制）</summary>
        public ControlFlag NewMask;
        /// <summary>当前生效控制中的最高等级（配置的 ControlLevel）</summary>
        public int Level;

        /// <summary>是否从"无控制"进入"被控制"</summary>
        public bool IsBegin => OldMask == ControlFlag.None && NewMask != ControlFlag.None;
        /// <summary>是否已完全解除控制</summary>
        public bool IsEnd => NewMask == ControlFlag.None;
    }
}