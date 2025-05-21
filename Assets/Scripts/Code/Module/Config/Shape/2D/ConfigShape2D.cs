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
        /// 点在形状内
        /// </summary>
        /// <param name="start">转换过坐标系的点</param>
        /// <param name="end">转换过坐标系的点</param>
        /// <returns></returns>
        public abstract bool ContainsLine(Vector2 start, Vector2 end);
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
        /// <summary>
        /// 获取网格数据
        /// </summary>
        /// <param name="triangles"></param>
        /// <param name="vertices"></param>
        public abstract void GetMeshData(List<int> triangles, List<Vector3> vertices);
        /// <summary>
        /// 获取包围盒最宽处宽度
        /// </summary>
        /// <returns></returns>
        public abstract float GetAABBRange();
    }
}