using Sirenix.OdinInspector;
namespace TaoTie
{
    public class PlatformReachPointEvt: IEventBase
    {
        /// <summary>
        /// 抵达寻路点actorid
        /// </summary>
        [LabelText("抵达寻路点ActorId")]
        [SceneGroupActorId]
        public int ActorId;

        /// <summary>
        /// 单位的寻路路径
        /// </summary>
        [SceneGroupRouterId]
        [LabelText("单位的寻路路径")]
        public int RouteId;

        /// <summary>
        /// 抵达的寻路点
        /// </summary>
        [LabelText("抵达的寻路点")]
        public int PointIndex;
    }
}