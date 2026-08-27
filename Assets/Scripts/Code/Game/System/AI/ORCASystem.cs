using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 解析法 ORCA（半平面 + LP）
    /// </summary>
    public class ORCASystem : IManager, IUpdate
    {
        private const int CAPACITY = 512;

        private AxisPair plane = AxisPair.XY; // 本项目使用 XY 平面

        public class Slot
        {
            public int id;
            public Vector3 position;
            public Vector3 prefVelocity;
            public Vector3 velocity;
            public float radius;
            public float maxSpeed;
            public float height;
            public float baseline;
            public int maxNeighbors;
            public float neighborDist;
            public float timeHorizon;
            public bool enabled;
        }

        private Slot[] slots = new Slot[CAPACITY];
        private int[] freeList = new int[CAPACITY];
        private int freeCount;
        private Dictionary<int, int> idToSlot = new Dictionary<int, int>();
        private List<int> activeList = new List<int>();
        private int nextId = 1;

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
        private JobHandle handle;

        public void Init()
        {
            plane = AxisPair.XY;
            for (int i = 0; i < CAPACITY; i++)
            {
                slots[i] = new Slot();
                freeList[i] = CAPACITY - 1 - i;
            }
            freeCount = CAPACITY;

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
        }

        public void Destroy()
        {
            if (!handle.IsCompleted)
                handle.Complete();

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

        #endregion

        public void Update()
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

                float range = s.radius + s.neighborDist;
                if (range > cellSize) cellSize = range;
            }

            nHashMap.Clear();

            var buildJob = new BuildHashJob
            {
                nActive = nActive,
                positions = nPositions,
                cellSize = cellSize,
                hashMap = nHashMap.AsParallelWriter()
            };
            JobHandle h = buildJob.Schedule(CAPACITY, 64);

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
                hashMap = nHashMap,
                cellSize = cellSize,
                timestep = dt,
                newVelocities = nNewVelocities
            };
            h = orcaJob.Schedule(CAPACITY, 64, h);

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
                // Nebukam 保留平面外轴分量：XY 保留 z，XZ 保留 y
                slots[slot].velocity = plane == AxisPair.XZ
                    ? new Vector3(rv.x, pref.y, rv.y)
                    : new Vector3(rv.x, rv.y, pref.z);
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
