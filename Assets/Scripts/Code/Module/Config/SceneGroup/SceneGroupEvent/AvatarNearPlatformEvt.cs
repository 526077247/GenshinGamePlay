using Sirenix.OdinInspector;
namespace TaoTie
{
    public class AvatarNearPlatformEvt: IEventBase
    {
        /// <summary>
        /// 靠近寻路单位的ActorId
        /// </summary>
        [SceneGroupActorId]
        [LabelText("靠近寻路单位的ActorId")]
        public int ActorId;

        /// <summary>
        /// 靠近单位的寻路路径
        /// </summary>
        [SceneGroupRouterId]
        [LabelText("靠近单位的寻路路径")]
        public int RouteId;

        /// <summary>
        /// 靠近单位的当前寻路点或下一个点的序号
        /// </summary>
        [LabelText("靠近单位的当前寻路点或下一个点的序号")]
        public int PointIndex;

        /// <summary>
        /// 靠近单位的是否正在移动
        /// </summary>
        [LabelText("靠近单位的是否正在移动")]
        public bool IsMoving;
    }
}