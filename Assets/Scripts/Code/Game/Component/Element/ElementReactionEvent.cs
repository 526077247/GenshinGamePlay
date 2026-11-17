namespace TaoTie
{
    /// <summary>
    /// 元素反应事件载荷（<see cref="MessageId.ElementReaction"/>）。
    ///
    /// 订阅键是宿主实体 Id（<see cref="ElementComponent.Id"/> 就是实体 Id），
    /// 和 <see cref="MessageId.EntityTagsChanged"/> 同一套广播口径：广播方用实体 Id 作 key，订阅方存的也是它。
    ///
    /// 载荷只描述"发生了什么"，不含任何效果：伤害 / 增伤由配置层的元素反应 Ability 自己算，
    /// 它通过 <see cref="ElementComponent.LastReactionId"/> 等属性读同一份数据（事件结构与属性一一对应）。
    /// </summary>
    public struct ElementReactionEvent
    {
        /// <summary>广播键：元素组件的 Id</summary>
        public long Id;

        /// <summary>反应表的 Id（<see cref="ElementReaction.Id"/>）</summary>
        public int ReactionId;

        /// <summary>本次刚挂上、引发反应的元素</summary>
        public int TriggerElement;

        /// <summary>被撞上的、原本就挂着的元素</summary>
        public int OtherElement;

        /// <summary>触发元素实际被扣掉的量</summary>
        public int TriggerConsumed;

        /// <summary>对方元素实际被扣掉的量</summary>
        public int OtherConsumed;
    }
}
