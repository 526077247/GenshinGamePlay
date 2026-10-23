using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 移动策略参数：玩家、AI、出生查询共用的半径、身高、足印采样与滑移参数。
    /// </summary>
    public sealed class ActorMoveProfile
    {
        public const float DefaultRadius = 0.5f;
        public const float DefaultHeight = 1f;
        public const float MinHeight = 0.1f;

        public const int DefaultFootprintSamples = 8;
        public const int DefaultNormalProbeCount = 16;
        public const int DefaultSlideBinaryIterations = 4;
        public const float DefaultSlideAngleStep = 10f;
        public const int DefaultSlideAngleSteps = 9;
        public const float DefaultEscapeProbeDistance = 8f;

        /// <summary>碰撞半径。</summary>
        public float Radius = DefaultRadius;

        /// <summary>身高，用于水体检测的竖直范围。</summary>
        public float Height = DefaultHeight;

        /// <summary>足印圆周采样点数。</summary>
        public int FootprintSamples = DefaultFootprintSamples;

        /// <summary>脱困时探测自由方向的数量。</summary>
        public int NormalProbeCount = DefaultNormalProbeCount;

        /// <summary>中心点二分次数。</summary>
        public int SlideBinaryIterations = DefaultSlideBinaryIterations;

        /// <summary>整步旋转候选的角度步长。</summary>
        public float SlideAngleStep = DefaultSlideAngleStep;

        /// <summary>整步旋转候选的最大角度档数。</summary>
        public int SlideAngleSteps = DefaultSlideAngleSteps;

        /// <summary>脱困时探测自由方向的最远距离。</summary>
        public float EscapeProbeDistance = DefaultEscapeProbeDistance;

        /// <summary>
        /// 从 Actor 配置创建移动参数；无配置时使用默认半径/身高。
        /// </summary>
        public static ActorMoveProfile Create(Actor actor)
        {
            var common = actor?.ConfigActor?.Common;
            return new ActorMoveProfile
            {
                Radius = common != null ? common.ModelRadius : DefaultRadius,
                Height = Mathf.Max(MinHeight, common != null ? common.ModelHeight : DefaultHeight),
            };
        }
    }
}
