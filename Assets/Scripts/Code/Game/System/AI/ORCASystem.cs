using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

namespace TaoTie
{
    /// <summary>
    /// 解析法 ORCA（半平面 + LP）
    /// </summary>
    public class ORCASystem : IManager, IUpdate
    {
        private const int CAPACITY = 512;
        private const int OBSTACLE_CAPACITY = 256;

        private AxisPair plane = AxisPair.XZ; // 本项目使用 XZ 平面（y 为竖直轴）

        public class Slot
        {
            public int id; // 实体唯一标识
            public Vector3 position; // 实体当前位置
            public Vector3 prefVelocity; // 期望/目标速度（寻路或AI发出的期望速度）
            public Vector3 velocity; // 实际速度（上帧计算出的最终速度）
            public float radius; // 实体半径
            public float maxSpeed; // 最大速度
            public float height; // 实体高度（用于纵向重叠判断）
            public float baseline; // 实体底部高度（y 轴坐标，与 height 构成纵向范围）
            public int maxNeighbors; // 避障时考虑的最大邻居数量
            public float neighborDist; // 邻居搜索半径
            public float timeHorizon; // 与动态代理（agent）避碰的时间范围，越大越提前避让
            public float radiusObst; // 对静态障碍物的膨胀半径
            public float timeHorizonObst; // 与静态障碍避碰的时间范围，越大越提前绕开障碍
            public bool enabled; // 是否参与避障（false 时直接沿期望速度移动）
        }

        public class ObstacleSlot
        {
            public int id;
            public List<float2> vertices = new List<float2>(); // 闭合顶点环（平面坐标，首尾同点）
            public float height; // 实体高度（用于纵向重叠判断）
            public float baseline; // 实体底部高度（y 轴坐标，与 height 构成纵向范围）
            public float thickness;
            public bool collisionEnabled;
        }

        private Slot[] slots = new Slot[CAPACITY];
        private int[] freeList = new int[CAPACITY];
        private int freeCount;
        private Dictionary<int, int> idToSlot = new Dictionary<int, int>();
        private List<int> activeList = new List<int>();
        private int nextId = 1;

        private ObstacleSlot[] obstacleSlots = new ObstacleSlot[OBSTACLE_CAPACITY];
        private int[] obstacleFreeList = new int[OBSTACLE_CAPACITY];
        private int obstacleFreeCount;
        private Dictionary<int, int> obstacleIdToSlot = new Dictionary<int, int>();
        private List<int> obstacleActiveList = new List<int>();
        private int nextObstacleId = 1;

        private NativeArray<float2> nPositions;
        private NativeArray<float2> nPrefVels;
        private NativeArray<float2> nVelocities;
        private NativeArray<float2> nNewVelocities;
        private NativeArray<float> nRadii;
        private NativeArray<float> nMaxSpeeds;
        private NativeArray<int> nMaxNeighbors;
        private NativeArray<float> nNeighborDist;
        private NativeArray<float> nTimeHorizon;
        private NativeArray<byte> nEnabled;
        private NativeArray<float> nBaseline;
        private NativeArray<float> nHeight;
        private NativeArray<byte> nActive;
        private NativeParallelMultiHashMap<int, int> nHashMap;
        private NativeArray<float> nRadiusObst;
        private NativeArray<float> nTimeHorizonObst;
        private NativeArray<ObstacleVertexData> nObstacleVertices;
        private NativeArray<ObstacleInfos> nObstacleInfos;
        private NativeParallelMultiHashMap<int, int> nObstacleHashMap;
        private JobHandle handle;
        private bool disposed;

        public void Init()
        {
            disposed = false;
            plane = AxisPair.XZ;
            for (int i = 0; i < CAPACITY; i++)
            {
                slots[i] = new Slot();
                freeList[i] = CAPACITY - 1 - i;
            }
            freeCount = CAPACITY;

            for (int i = 0; i < OBSTACLE_CAPACITY; i++)
            {
                obstacleSlots[i] = new ObstacleSlot();
                obstacleFreeList[i] = OBSTACLE_CAPACITY - 1 - i;
            }
            obstacleFreeCount = OBSTACLE_CAPACITY;

            nPositions = new NativeArray<float2>(CAPACITY, Allocator.Persistent);
            nPrefVels = new NativeArray<float2>(CAPACITY, Allocator.Persistent);
            nVelocities = new NativeArray<float2>(CAPACITY, Allocator.Persistent);
            nNewVelocities = new NativeArray<float2>(CAPACITY, Allocator.Persistent);
            nRadii = new NativeArray<float>(CAPACITY, Allocator.Persistent);
            nMaxSpeeds = new NativeArray<float>(CAPACITY, Allocator.Persistent);
            nMaxNeighbors = new NativeArray<int>(CAPACITY, Allocator.Persistent);
            nNeighborDist = new NativeArray<float>(CAPACITY, Allocator.Persistent);
            nTimeHorizon = new NativeArray<float>(CAPACITY, Allocator.Persistent);
            nEnabled = new NativeArray<byte>(CAPACITY, Allocator.Persistent);
            nBaseline = new NativeArray<float>(CAPACITY, Allocator.Persistent);
            nHeight = new NativeArray<float>(CAPACITY, Allocator.Persistent);
            nActive = new NativeArray<byte>(CAPACITY, Allocator.Persistent);
            nHashMap = new NativeParallelMultiHashMap<int, int>(CAPACITY * 8, Allocator.Persistent);
            nRadiusObst = new NativeArray<float>(CAPACITY, Allocator.Persistent);
            nTimeHorizonObst = new NativeArray<float>(CAPACITY, Allocator.Persistent);
            EnsureObstacleBuffers(1024, 64);
            CodeLoader.Instance.OnApplicationQuit += DisposeAll;
        }

        public void Destroy()
        {
            DisposeAll();
            CodeLoader.Instance.OnApplicationQuit -= DisposeAll;
        }

        private void DisposeAll()
        {
            if (disposed) return;
            disposed = true;
            try
            {
                if (!handle.IsCompleted)
                    handle.Complete();
            }
            catch (System.Exception e)
            {
                Log.Error(e);
            }

            if (nPositions.IsCreated) nPositions.Dispose();
            if (nPrefVels.IsCreated) nPrefVels.Dispose();
            if (nVelocities.IsCreated) nVelocities.Dispose();
            if (nNewVelocities.IsCreated) nNewVelocities.Dispose();
            if (nRadii.IsCreated) nRadii.Dispose();
            if (nMaxSpeeds.IsCreated) nMaxSpeeds.Dispose();
            if (nMaxNeighbors.IsCreated) nMaxNeighbors.Dispose();
            if (nNeighborDist.IsCreated) nNeighborDist.Dispose();
            if (nTimeHorizon.IsCreated) nTimeHorizon.Dispose();
            if (nEnabled.IsCreated) nEnabled.Dispose();
            if (nBaseline.IsCreated) nBaseline.Dispose();
            if (nHeight.IsCreated) nHeight.Dispose();
            if (nActive.IsCreated) nActive.Dispose();
            if (nHashMap.IsCreated) nHashMap.Dispose();
            if (nRadiusObst.IsCreated) nRadiusObst.Dispose();
            if (nTimeHorizonObst.IsCreated) nTimeHorizonObst.Dispose();
            if (nObstacleVertices.IsCreated) nObstacleVertices.Dispose();
            if (nObstacleInfos.IsCreated) nObstacleInfos.Dispose();
            if (nObstacleHashMap.IsCreated) nObstacleHashMap.Dispose();
            handle = default;
        }

        #region 公共 API（供 ORCAAgentComponent 调用）

        public Slot AddEntity(Vector3 position, float radius, float height)
        {
            if (freeCount == 0)
            {
                Log.Error("ORCAManager capacity exceeded");
                return null;
            }
            int slot = freeList[--freeCount];
            int id = nextId++;
            Slot s = slots[slot];
            s.id = id;
            s.position = position;
            s.radius = radius;
            s.height = height;
            s.maxSpeed = 20f;
            s.prefVelocity = Vector3.zero;
            s.velocity = Vector3.zero;
            s.maxNeighbors = 15;
            s.neighborDist = 20f;
            s.timeHorizon = 15f;
            s.baseline = 0f;
            s.radiusObst = radius;
            s.timeHorizonObst = 2f;
            s.enabled = false;
            idToSlot[id] = slot;
            activeList.Add(slot);
            nActive[slot] = 1;
            return s;
        }

        public void RemoveEntity(int id)
        {
            if (!idToSlot.TryGetValue(id, out int slot))
                return;
            idToSlot.Remove(id);
            activeList.Remove(slot);
            freeList[freeCount++] = slot;
            nActive[slot] = 0;
            slots[slot].id = 0;
        }

        public void SetEnable(int id, bool enable)
        {
            if (idToSlot.TryGetValue(id, out int slot))
                slots[slot].enabled = enable;
        }

        public void SetRadius(int id, float radius)
        {
            if (idToSlot.TryGetValue(id, out int slot))
                slots[slot].radius = radius;
        }

        public void SetHeight(int id, float height)
        {
            if (idToSlot.TryGetValue(id, out int slot))
                slots[slot].height = height;
        }

        public void SetVelocity(int id, Vector3 velocity, float maxSpeed)
        {
            if (!idToSlot.TryGetValue(id, out int slot))
                return;
            slots[slot].prefVelocity = velocity;
            slots[slot].maxSpeed = maxSpeed;
        }

        public void SetPosition(int id, Vector3 position)
        {
            if (idToSlot.TryGetValue(id, out int slot))
                slots[slot].position = position;
        }

        public Vector3 GetVelocity(int id)
        {
            if (idToSlot.TryGetValue(id, out int slot))
                return slots[slot].velocity;
            return Vector3.zero;
        }

        public void SetPlane(AxisPair p)
        {
            plane = p;
        }

        public void SetRadiusObst(int id, float radiusObst)
        {
            if (idToSlot.TryGetValue(id, out int slot))
                slots[slot].radiusObst = radiusObst;
        }

        public void SetTimeHorizonObst(int id, float timeHorizonObst)
        {
            if (idToSlot.TryGetValue(id, out int slot))
                slots[slot].timeHorizonObst = timeHorizonObst;
        }

        #endregion

        #region 障碍注册/移除 API

        /// <summary>
        /// 注册一个静态障碍多边形。
        /// </summary>
        /// <param name="vertices">多边形顶点（同一平面上的点，沿包围顺序）。</param>
        /// <param name="height">障碍高度（沿竖直轴）。</param>
        /// <param name="baseline">障碍底部所在的竖直高度。</param>
        /// <param name="thickness">障碍厚度，并入智能体避障半径。</param>
        /// <param name="inverseOrder">true 时不自动规整绕序，保持入参方向（可控制“可通行侧”），默认自动规整为内部在左。</param>
        /// <param name="maxSegmentLength">边长超过该值时自动细分，0 表示不细分。</param>
        public int AddObstacle(IList<Vector3> vertices, float height = 1f, float baseline = 0f,
            float thickness = 0f, bool inverseOrder = false, float maxSegmentLength = 10f)
        {
            if (vertices == null || vertices.Count < 2)
            {
                Log.Error("AddObstacle: need at least 2 vertices");
                return 0;
            }
            if (height <= 0f || thickness < 0f)
            {
                Log.Error("AddObstacle: height must be > 0");
                return 0;
            }
            if (obstacleFreeCount == 0)
            {
                Log.Error("ORCASystem obstacle capacity exceeded");
                return 0;
            }

            List<float2> pts = new List<float2>(vertices.Count + 8);
            for (int i = 0; i < vertices.Count; i++)
                pts.Add(ToPlane(vertices[i]));

            if (inverseOrder)
            {
                pts.Reverse();
            }
            else if (SignedArea(pts) < 0f)
            {
                // 自动规整为 CCW（内部在左，RVO2 要求）
                pts.Reverse();
            }

            // 自动闭合
            if (lengthsq(pts[0] - pts[pts.Count - 1]) > 1e-6f)
                pts.Add(pts[0]);

            if (pts.Count < 3)
            {
                Log.Error("AddObstacle: degenerate polygon");
                return 0;
            }

            // 细分长边
            if (maxSegmentLength > 1e-4f)
                pts = Subdivide(pts, maxSegmentLength);

            int slot = obstacleFreeList[--obstacleFreeCount];
            ObstacleSlot o = obstacleSlots[slot];
            o.id = nextObstacleId++;
            o.height = height;
            o.baseline = baseline;
            o.thickness = thickness;
            o.collisionEnabled = true;
            o.vertices.Clear();
            o.vertices.AddRange(pts);
            obstacleIdToSlot[o.id] = slot;
            obstacleActiveList.Add(slot);
            return o.id;
        }

        /// <summary>
        /// 注册一个圆形障碍（内接正多边形近似）。
        /// </summary>
        public int AddObstacleCircle(Vector3 center, float radius, float height, int sides = 16,
            float baseline = 0f, float thickness = 0f)
        {
            if (radius <= 0f || height <= 0f)
            {
                Log.Error("AddObstacleCircle: radius/height must be > 0");
                return 0;
            }
            if (sides < 3) sides = 3;
            List<Vector3> verts = new List<Vector3>(sides);
            for (int i = 0; i < sides; i++)
            {
                float angle = (2f * Mathf.PI * i) / sides;
                verts.Add(plane == AxisPair.XZ
                    ? new Vector3(center.x + Mathf.Cos(angle) * radius, center.y, center.z + Mathf.Sin(angle) * radius)
                    : new Vector3(center.x + Mathf.Cos(angle) * radius, center.y + Mathf.Sin(angle) * radius, center.z));
            }
            return AddObstacle(verts, height, baseline, thickness, false, 0f);
        }

        /// <summary>
        /// 注册一个矩形障碍（平面内宽 extents.x、进深 extents.y，绕竖直轴旋转 yawRadians）。
        /// </summary>
        public int AddObstacleBox(Vector3 center, Vector3 extents, float height, float yawRadians = 0f,
            float baseline = 0f, float thickness = 0f)
        {
            if (extents.x <= 0f || extents.y <= 0f || height <= 0f)
            {
                Log.Error("AddObstacleBox: extents/height must be > 0");
                return 0;
            }
            float cos = Mathf.Cos(yawRadians);
            float sin = Mathf.Sin(yawRadians);
            Vector3 fwd = plane == AxisPair.XZ ? new Vector3(sin, 0f, cos) : new Vector3(cos, sin, 0f);
            Vector3 right = plane == AxisPair.XZ ? new Vector3(cos, 0f, -sin) : new Vector3(-sin, cos, 0f);
            Vector3 hf = fwd * (extents.y * 0.5f);
            Vector3 hr = right * (extents.x * 0.5f);
            List<Vector3> verts = new List<Vector3>(4)
            {
                center + hr + hf,
                center - hr + hf,
                center - hr - hf,
                center + hr - hf
            };
            return AddObstacle(verts, height, baseline, thickness, false, 0f);
        }

        public void RemoveObstacle(int id)
        {
            if (!obstacleIdToSlot.TryGetValue(id, out int slot))
                return;
            obstacleIdToSlot.Remove(id);
            obstacleActiveList.Remove(slot);
            obstacleFreeList[obstacleFreeCount++] = slot;
            obstacleSlots[slot].id = 0;
        }

        public void ClearObstacles()
        {
            for (int k = 0; k < obstacleActiveList.Count; k++)
            {
                obstacleIdToSlot.Remove(obstacleSlots[obstacleActiveList[k]].id);
                obstacleSlots[obstacleActiveList[k]].id = 0;
            }
            obstacleActiveList.Clear();
            for (int i = 0; i < OBSTACLE_CAPACITY; i++)
                obstacleFreeList[i] = OBSTACLE_CAPACITY - 1 - i;
            obstacleFreeCount = OBSTACLE_CAPACITY;
        }

        public void SetObstacleEnabled(int id, bool enabled)
        {
            if (obstacleIdToSlot.TryGetValue(id, out int slot))
                obstacleSlots[slot].collisionEnabled = enabled;
        }

        public int GetObstacleCount()
        {
            return obstacleActiveList.Count;
        }

        #endregion

        /// <summary>
        /// 将障碍 Slot 打包进 Native 数组，返回打包后的顶点（边）数。
        /// </summary>
        private int PackObstacles()
        {
            int vertCount = 0;
            int infoCount = 0;
            for (int k = 0; k < obstacleActiveList.Count; k++)
            {
                ObstacleSlot o = obstacleSlots[obstacleActiveList[k]];
                if (o.vertices == null || o.vertices.Count < 3)
                    continue;

                ObstacleInfos infos = new ObstacleInfos
                {
                    height = o.height,
                    baseline = o.baseline,
                    thickness = o.thickness,
                    collisionEnabled = (byte)(o.collisionEnabled ? 1 : 0)
                };
                int infoIdx = infoCount;
                nObstacleInfos[infoIdx] = infos;
                infoCount++;

                int baseIdx = vertCount;
                int n = o.vertices.Count;   // 闭合环，实际边数 = n - 1
                int edgeCount = n - 1;
                for (int i = 0; i < edgeCount; i++)
                {
                    float2 cur = o.vertices[i];
                    float2 nextP = o.vertices[i + 1];
                    float2 prevP = o.vertices[(i == 0) ? edgeCount - 1 : i - 1];
                    float2 d = nextP - cur;
                    float len = length(d);
                    float2 dir = len > 1e-6f ? d / len : new float2(1f, 0f);
                    float detV = (cur.x - prevP.x) * (nextP.y - cur.y) - (cur.y - prevP.y) * (nextP.x - cur.x);
                    nObstacleVertices[baseIdx + i] = new ObstacleVertexData
                    {
                        pos = cur,
                        dir = dir,
                        prev = baseIdx + ((i == 0) ? edgeCount - 1 : i - 1),
                        next = baseIdx + ((i == edgeCount - 1) ? 0 : i + 1),
                        index = baseIdx + i,
                        convex = (byte)(detV >= 0f ? 1 : 0),
                        infos = infoIdx
                    };
                }
                vertCount += edgeCount;
            }
            return vertCount;
        }

        /// <summary>
        /// 确保障碍 Native 数组容量足够，不足时在主线程扩倍重建。
        /// </summary>
        private void EnsureObstacleBuffers(int vertices, int infos)
        {
            int vc = max(vertices, 1);
            int ic = max(infos, 1);
            if (!nObstacleVertices.IsCreated)
            {
                nObstacleVertices = new NativeArray<ObstacleVertexData>(vc, Allocator.Persistent);
                nObstacleInfos = new NativeArray<ObstacleInfos>(ic, Allocator.Persistent);
                nObstacleHashMap = new NativeParallelMultiHashMap<int, int>(vc * 4 + 16, Allocator.Persistent);
                return;
            }
            if (nObstacleVertices.Length >= vc && nObstacleInfos.Length >= ic && nObstacleHashMap.Capacity >= vc * 4)
                return;
            if (!handle.IsCompleted)
                handle.Complete();
            int growVc = max(vc, nObstacleVertices.Length * 2);
            int growIc = max(ic, nObstacleInfos.Length * 2);
            nObstacleVertices.Dispose();
            nObstacleInfos.Dispose();
            nObstacleHashMap.Dispose();
            nObstacleVertices = new NativeArray<ObstacleVertexData>(growVc, Allocator.Persistent);
            nObstacleInfos = new NativeArray<ObstacleInfos>(growIc, Allocator.Persistent);
            nObstacleHashMap = new NativeParallelMultiHashMap<int, int>(growVc * 4 + 16, Allocator.Persistent);
        }

        private static float SignedArea(List<float2> pts)
        {
            float area = 0f;
            for (int i = 0; i + 1 < pts.Count; i++)
                area += pts[i].x * pts[i + 1].y - pts[i].y * pts[i + 1].x;
            return area * 0.5f;
        }

        private static List<float2> Subdivide(List<float2> pts, float maxSegmentLength)
        {
            List<float2> result = new List<float2>(pts.Count * 2);
            for (int i = 0; i + 1 < pts.Count; i++)
            {
                float2 a = pts[i];
                float2 b = pts[i + 1];
                result.Add(a);
                float len = length(b - a);
                int parts = (int)ceil(len / maxSegmentLength);
                if (parts > 1)
                {
                    for (int p = 1; p < parts; p++)
                        result.Add(a + (b - a) * (p / (float)parts));
                }
            }
            result.Add(pts[pts.Count - 1]);
            return result;
        }

        public void Update()
        {
            try
            {
                int n = activeList.Count;
                if (n == 0)
                {
                    if (!handle.IsCompleted) handle.Complete();
                    return;
                }
                if (!handle.IsCompleted) handle.Complete();

                float dt = GameTimerManager.Instance.GetDeltaTime() / 1000f;
                if (dt <= 0f) dt = 0.016f;

                float cellSize = 1f;
                for (int k = 0; k < n; k++)
                {
                    int slot = activeList[k];
                    Slot s = slots[slot];
                    nPositions[slot] = ToPlane(s.position);
                    nPrefVels[slot] = ToPlane(s.prefVelocity);
                    nVelocities[slot] = ToPlane(s.velocity);
                    nRadii[slot] = s.radius;
                    nMaxSpeeds[slot] = s.maxSpeed;
                    nMaxNeighbors[slot] = s.maxNeighbors;
                    nNeighborDist[slot] = s.neighborDist;
                    nTimeHorizon[slot] = s.timeHorizon;
                    nEnabled[slot] = (byte)(s.enabled ? 1 : 0);
                    nBaseline[slot] = s.baseline;
                    nHeight[slot] = s.height;
                    nRadiusObst[slot] = s.radiusObst;
                    nTimeHorizonObst[slot] = s.timeHorizonObst;

                    float range = s.radius + s.neighborDist;
                    if (range > cellSize) cellSize = range;
                }

                // 确保障碍 Native 数组容量，再打包（主线程）
                int reqVerts = 0;
                int reqInfos = obstacleActiveList.Count;
                for (int k = 0; k < obstacleActiveList.Count; k++)
                {
                    List<float2> ov = obstacleSlots[obstacleActiveList[k]].vertices;
                    if (ov != null && ov.Count >= 3)
                        reqVerts += ov.Count - 1;
                }
                EnsureObstacleBuffers(reqVerts, reqInfos);
                int obstacleVertexCount = PackObstacles();

                nHashMap.Clear();
                nObstacleHashMap.Clear();

                var buildJob = new BuildHashJob
                {
                    nActive = nActive,
                    positions = nPositions,
                    cellSize = cellSize,
                    hashMap = nHashMap.AsParallelWriter()
                };
                JobHandle h = buildJob.Schedule(CAPACITY, 64);

                var obstacleBuildJob = new BuildObstacleHashJob
                {
                    vertices = nObstacleVertices,
                    count = obstacleVertexCount,
                    cellSize = cellSize,
                    hashMap = nObstacleHashMap.AsParallelWriter()
                };
                JobHandle hObs = obstacleBuildJob.Schedule(max(1, obstacleVertexCount), 64);

                var orcaJob = new ORCALinesJob
                {
                    nActive = nActive,
                    positions = nPositions,
                    prefVels = nPrefVels,
                    velocities = nVelocities,
                    radii = nRadii,
                    maxSpeeds = nMaxSpeeds,
                    maxNeighborsArr = nMaxNeighbors,
                    neighborDistArr = nNeighborDist,
                    timeHorizonArr = nTimeHorizon,
                    enableds = nEnabled,
                    baselines = nBaseline,
                    heights = nHeight,
                    obstacleCount = obstacleVertexCount,
                    obstacleVertices = nObstacleVertices,
                    obstacleInfos = nObstacleInfos,
                    obstacleHashMap = nObstacleHashMap,
                    radiusObsts = nRadiusObst,
                    timeHorizonObsts = nTimeHorizonObst,
                    hashMap = nHashMap,
                    cellSize = cellSize,
                    timestep = dt,
                    newVelocities = nNewVelocities
                };
                h = orcaJob.Schedule(CAPACITY, 64, JobHandle.CombineDependencies(h, hObs));

                var applyJob = new ApplyJob
                {
                    nActive = nActive,
                    newVelocities = nNewVelocities,
                    velocities = nVelocities
                };
                h = applyJob.Schedule(CAPACITY, 64, h);

                handle = h;
                handle.Complete();

                for (int k = 0; k < n; k++)
                {
                    int slot = activeList[k];
                    float2 rv = nVelocities[slot];
                    Vector3 pref = slots[slot].prefVelocity;
                    float2 prefPlane2 = plane == AxisPair.XZ ? new float2(pref.x, pref.z) : new float2(pref.x, pref.y);
                    float rvMagSq = lengthsq(rv);
                    if (rvMagSq < 0.25f && lengthsq(prefPlane2) > 1e-6f)
                    {
                        // ORCA 结果过小说明被障碍（或密集人群）完全挡住。
                        // 沿用 ORCA 已求出的可行方向（被挡时即障碍切向）保持贴边滑动；
                        // 完全无可行方向时退回期望方向，交由 ClampWallBlock/GetEscapeDirection 做实体滑动与脱困。
                        float2 slideDir = rvMagSq > 1e-4f ? rv / sqrt(rvMagSq) : normalize(prefPlane2);
                        float keepSpeed = min(length(prefPlane2), nMaxSpeeds[slot]);
                        rv = slideDir * keepSpeed;
                    }
                    slots[slot].velocity = plane == AxisPair.XZ
                        ? new Vector3(rv.x, pref.y, rv.y)
                        : new Vector3(rv.x, rv.y, pref.z);
                }
            }
            catch (System.Exception e)
            {
                handle = default;
                Log.Error(e);
            }
        }

        private float2 ToPlane(Vector3 p)
        {
            return plane == AxisPair.XZ ? new float2(p.x, p.z) : new float2(p.x, p.y);
        }

        private Vector3 FromPlane(float2 v)
        {
            return plane == AxisPair.XZ ? new Vector3(v.x, 0f, v.y) : new Vector3(v.x, v.y, 0f);
        }
    }
}
