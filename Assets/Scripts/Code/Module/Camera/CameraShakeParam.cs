using UnityEngine;

namespace TaoTie
{
    public class CameraShakeParam
    {
        public long Id;
        /// <summary>
        /// 振动源
        /// </summary>
        public Vector3 Source;
        /// <summary>
        /// 震动幅度
        /// </summary>
        public float ShakeRange;
        /// <summary>
        /// 震动时间
        /// </summary>
        public int ShakeTime;
        /// <summary>
        /// 震动事件广播距离
        /// </summary>
        public float ShakeDistance;
        /// <summary>
        /// 震动频率(Hz)
        /// </summary>
        public int ShakeFrequency;
        /// <summary>
        /// 震动方向
        /// </summary>
        public Vector3 ShakeDir;
        /// <summary>
        /// 衰减范围
        /// </summary>
        public float RangeAttenuation;
        /// <summary>
        /// 开始震动时间
        /// </summary>
        public long StartTime;
    }
}