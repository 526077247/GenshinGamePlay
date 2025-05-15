using Nino.Core;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
    public abstract partial class ConfigShape
    {
        public abstract Collider CreateCollider(GameObject obj, bool isTrigger);
        
        /// <summary>
        /// 点在形状内
        /// </summary>
        /// <param name="target">转换过坐标系的点</param>
        /// <returns></returns>
        public abstract bool Contains(Vector3 target);
        /// <summary>
        /// 线与形状相交
        /// </summary>
        /// <param name="start">转换过坐标系的点</param>
        /// <param name="end">转换过坐标系的点</param>
        /// <returns></returns>
        public abstract bool ContainsLine(Vector3 start, Vector3 end);
        /// <summary>
        /// 距离
        /// </summary>
        /// <param name="target">转换过坐标系的点</param>
        /// <returns></returns>
        public abstract float Distance(Vector3 target);
        
        /// <summary>
        /// 距离平方
        /// </summary>
        /// <param name="target">转换过坐标系的点</param>
        /// <returns></returns>
        public abstract float SqrMagnitude(Vector3 target);
        /// <summary>
        /// 距离平方
        /// </summary>
        /// <param name="target">转换过坐标系的点</param>
        /// <param name="inner">是否在范围内</param>
        /// <returns></returns>
        public abstract float SqrMagnitude(Vector3 target, out bool inner);

        public abstract int RaycastHitInfo(Vector3 pos, Quaternion rot,EntityType[] filter,out HitInfo[] hitInfos);
    }
}