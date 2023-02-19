namespace TaoTie
{
    public struct HeroNearPlatformEvt: IEventBase
    {
        /// <summary>
        /// 靠近单位的actorid
        /// </summary>
        [GearActorId]
        public int actorId;

        /// <summary>
        /// 靠近单位的寻路路径
        /// </summary>
        [GearRouterId]
        public int routeId;

        /// <summary>
        /// 靠近单位的当前寻路点或下一个点的序号
        /// </summary>
        public int pointIndex;

        /// <summary>
        /// 靠近单位的是否正在移动
        /// </summary>
        public bool isMoving;
    }
}