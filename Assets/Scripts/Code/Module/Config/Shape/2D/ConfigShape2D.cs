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

        public abstract void GetMeshData(List<int> triangles, List<Vector3> vertices);
    }
}