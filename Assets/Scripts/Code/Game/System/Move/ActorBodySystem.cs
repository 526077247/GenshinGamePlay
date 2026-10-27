using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 角色体积代理注册表（角色间互斥的宽相位）。
    ///
    /// 为什么单独一套而不是用物理世界：
    /// 单位位置由 MoveComponent 直写 <see cref="SceneEntity.Position"/>，工程
    /// <c>m_AutoSyncTransforms = 0</c>，物理查询拿到的是上一物理步的位置；且部分资产没有碰撞体。
    /// 所以这里的宽相位直接用逻辑位置 + 配置代理重建，每物理步重建一次（O(单位数 × 代理数)）。
    ///
    /// 与 <see cref="ORCASystem"/> 的分工：ORCA 做「未来碰撞」的速度层避让（软），
    /// 本系统做「已经接触/即将接触」的位置与冲量解算（硬）。两者互不依赖。
    /// </summary>
    public class ActorBodySystem : IManager
    {
        private const float MinCellSize = 0.5f;
        private const int MaxCellsPerAxis = 64;

        private List<ActorBodyComponent> bodies;
        private Dictionary<long, List<int>> grid;
        private int[] stamp;
        private int stampId;
        private float cellSize = MinCellSize;
        private float builtTime = float.NaN;
        private bool dirty = true;

        /// <summary>全局开关：关掉后角色之间完全不互斥（用于 A/B 对比与线上热关）。</summary>
        public bool Enabled = true;

        public int Count => bodies != null ? bodies.Count : 0;

        /// <summary>当前注册单位中最大的代理半径（查询扩边用）。</summary>
        public float MaxBodyRadius { get; private set; } = 0.3f;

        /// <summary>当前注册单位中最大的代理水平偏移（长条/多足体型查询扩边用）。</summary>
        public float MaxBodyOffset { get; private set; }

        public void Init()
        {
            bodies = new List<ActorBodyComponent>(32);
            grid = new Dictionary<long, List<int>>(128);
            stamp = new int[32];
            stampId = 0;
            cellSize = MinCellSize;
            builtTime = float.NaN;
            dirty = true;
        }

        public void Destroy()
        {
            bodies?.Clear();
            grid?.Clear();
            bodies = null;
            grid = null;
            stamp = null;
            builtTime = float.NaN;
            dirty = true;
        }

        public void Register(ActorBodyComponent body)
        {
            if (body == null || bodies == null) return;
            if (!bodies.Contains(body)) bodies.Add(body);
            if (stamp.Length < bodies.Count) System.Array.Resize(ref stamp, Mathf.NextPowerOfTwo(bodies.Count));
            dirty = true;
        }

        public void Unregister(ActorBodyComponent body)
        {
            if (body == null || bodies == null) return;
            if (bodies.Remove(body)) dirty = true;
        }

        /// <summary>
        /// 查询水平圆形范围内的候选代理持有者（只做宽相位，精确重叠由调用方按代理逐个判定）。
        /// </summary>
        public int QueryNeighbors(Vector3 position, float radius, List<ActorBodyComponent> results)
        {
            results.Clear();
            if (bodies == null || bodies.Count == 0) return 0;

            RebuildIfNeeded();

            float r = Mathf.Max(0f, radius);
            int minX = Mathf.FloorToInt((position.x - r) / cellSize);
            int maxX = Mathf.FloorToInt((position.x + r) / cellSize);
            int minZ = Mathf.FloorToInt((position.z - r) / cellSize);
            int maxZ = Mathf.FloorToInt((position.z + r) / cellSize);
            if (maxX - minX >= MaxCellsPerAxis || maxZ - minZ >= MaxCellsPerAxis)
                return 0;

            stampId++;
            if (stampId == int.MaxValue) stampId = 1;

            for (int x = minX; x <= maxX; x++)
            {
                for (int z = minZ; z <= maxZ; z++)
                {
                    if (!grid.TryGetValue(Key(x, z), out List<int> list)) continue;
                    for (int i = 0; i < list.Count; i++)
                    {
                        int idx = list[i];
                        if (idx < 0 || idx >= bodies.Count) continue;
                        if (stamp[idx] == stampId) continue;
                        stamp[idx] = stampId;
                        results.Add(bodies[idx]);
                    }
                }
            }

            return results.Count;
        }

        private static long Key(int x, int z) => ((long)x << 32) ^ (uint)z;

        /// <summary>
        /// 每物理步重建一次：位置与代理都是运行时数据，且单位在一帧内可能多次查询。
        /// 用 Time.fixedTime 做步识别（同一物理步内多次调用只重建一次）。
        /// </summary>
        private void RebuildIfNeeded()
        {
            float now = Time.fixedTime;
            if (!dirty && Mathf.Abs(now - builtTime) < 1e-6f) return;

            builtTime = now;
            dirty = false;

            float maxRadius = 0f;
            float maxOffset = 0f;
            for (int i = 0; i < bodies.Count; i++)
            {
                ActorBodyComponent b = bodies[i];
                if (b == null) continue;
                maxRadius = Mathf.Max(maxRadius, b.MaxProxyRadius);
                maxOffset = Mathf.Max(maxOffset, b.MaxProxyOffset);
            }
            cellSize = Mathf.Max(MinCellSize, maxRadius * 2f);
            MaxBodyRadius = Mathf.Max(0.01f, maxRadius);
            MaxBodyOffset = maxOffset;

            // 复用 cell 列表（只清空不重建），避免每物理步产生 GC
            foreach (KeyValuePair<long, List<int>> kv in grid) kv.Value.Clear();

            for (int i = 0; i < bodies.Count; i++)
            {
                ActorBodyComponent b = bodies[i];
                if (b == null) continue;

                Vector3 pos = b.WorldCenter;
                float bound = b.MaxProxyRadius + b.MaxProxyOffset;
                int minX = Mathf.FloorToInt((pos.x - bound) / cellSize);
                int maxX = Mathf.FloorToInt((pos.x + bound) / cellSize);
                int minZ = Mathf.FloorToInt((pos.z - bound) / cellSize);
                int maxZ = Mathf.FloorToInt((pos.z + bound) / cellSize);

                for (int x = minX; x <= maxX; x++)
                {
                    for (int z = minZ; z <= maxZ; z++)
                    {
                        long key = Key(x, z);
                        if (!grid.TryGetValue(key, out List<int> list))
                        {
                            list = new List<int>(4);
                            grid.Add(key, list);
                        }
                        list.Add(i);
                    }
                }
            }
        }
    }
}
