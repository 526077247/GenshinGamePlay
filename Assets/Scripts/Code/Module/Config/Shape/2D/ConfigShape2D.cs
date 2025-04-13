using System.Collections.Generic;
using Nino.Core;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
    public abstract partial class ConfigShape2D
    {
        /// <summary>
        /// 点在形状内
        /// </summary>
        /// <param name="target">转换过坐标系的点</param>
        /// <returns></returns>
        public abstract bool Contains(Vector2 target);
        
        /// <summary>
        /// 距离
        /// </summary>
        /// <param name="target">转换过坐标系的点</param>
        /// <returns></returns>
        public abstract float Distance(Vector2 target);

        /// <summary>
        /// 距离平方
        /// </summary>
        /// <param name="target">转换过坐标系的点</param>
        /// <returns></returns>
        public abstract float SqrMagnitude(Vector2 target);
        /// <summary>
        /// 距离平方
        /// </summary>
        /// <param name="target">转换过坐标系的点</param>
        /// <param name="inner">是否在范围内</param>
        /// <returns></returns>
        public abstract float SqrMagnitude(Vector2 target, out bool inner);
        
        public abstract void GetMeshData(List<int> triangles, List<Vector3> vertices);
    }
}