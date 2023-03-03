namespace TaoTie
{
    public struct PlatformReachPointEvt: IEventBase
    {
        /// <summary>
        /// 靠近单位的actorid
        /// </summary>
        [SceneGroupActorId]
        public int actorId;

        /// <summary>
        /// 靠近单位的寻路路径
        /// </summary>
        [SceneGroupRouterId]
        public int routeId;

        /// <summary>
        /// 靠近单位的当前寻路点或下一个点的序号
        /// </summary>
        public int pointIndex;
    }
}