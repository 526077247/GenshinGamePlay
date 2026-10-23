using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using DotRecast.Core;
using DotRecast.Detour;
using DotRecast.Detour.Io;
using DotRecast.Recast;
using DotRecast.Recast.Geom;
using UnityEngine;

namespace TaoTie
{
    public class NavmeshSystem:IManager, IUpdate
    {
        const int MAX_POLYS = 256;

        /// <summary>
        /// 临时 NavMesh 的查询对象封装（不持有 chunk 引用）。
        /// </summary>
        private sealed class TemporaryNavMesh
        {
            public readonly DtNavMeshQuery Query;

            public TemporaryNavMesh(DtNavMeshQuery query)
            {
                Query = query;
            }
        }

        /// <summary>
        /// 后台构建任务：输入为不可变快照，结果经主线程 <see cref="Update"/> 落库。
        /// </summary>
        private sealed class PendingTemporaryBuild
        {
            public readonly string Name;
            public readonly List<float> Vertices;
            public readonly List<int> Triangles;
            public readonly ETTask<bool> Task;
            public TemporaryNavMesh NavMesh;
            public string Error;

            public PendingTemporaryBuild(string name, List<float> vertices, List<int> triangles, ETTask<bool> task)
            {
                Name = name;
                Vertices = vertices;
                Triangles = triangles;
                Task = task;
            }
        }

        /// <summary>
        /// 临时 NavMesh 构建配置模板
        /// </summary>
        private static readonly RcConfig TempBuildConfig = new RcConfig(
            RcPartition.MONOTONE,
            0.5f, 0.1f,
            45f, 2f, 0.5f, 0.2f,
            6, 8,
            4f, 0.4f,
            6,
            2f, 0.5f,
            true, true, true, new RcAreaModification(1), true);
        
        public static NavmeshSystem Instance;
        private readonly Dictionary<string, DtNavMeshQuery> navmeshs = new Dictionary<string, DtNavMeshQuery>();
        private readonly Dictionary<string, TemporaryNavMesh> temporaryNavmeshs = new Dictionary<string, TemporaryNavMesh>();
        private readonly object buildLock = new object();
        private readonly Dictionary<string, PendingTemporaryBuild> pendingBuilds = new Dictionary<string, PendingTemporaryBuild>();
        private readonly List<PendingTemporaryBuild> completedBuilds = new List<PendingTemporaryBuild>();
        private readonly HashSet<string> canceledBuilds = new HashSet<string>();
        private readonly DtQueryDefaultFilter filter = new DtQueryDefaultFilter();
        private RcVec3f extents = new RcVec3f(15, 10, 15);
        private List<long> polys = new List<long>(MAX_POLYS);
        private List<StraightPathItem> straightPath = new List<StraightPathItem>();
        public void Init()
        {
            Instance = this;
        }

        public void Destroy()
        {
            Instance = null;
        }

        /// <summary>
        /// 预加载并缓存指定名称的 NavMesh 查询对象（离线表或临时表），
        /// 供地形/水面查询 Provider 在同步路径上安全使用。已加载则直接返回 true。
        /// </summary>
        public async ETTask<bool> Preload(string name, ETCancellationToken token = null)
        {
            if (string.IsNullOrEmpty(name)) return false;
            if (TryGetLoadedQuery(name, out _)) return true;
            // 与 Find 使用同一把按名字的锁，避免与寻路查询并发重复加载同一份数据。
            using (await CoroutineLockManager.Instance.Wait(CoroutineLockType.PathQuery, name.GetHashCode()))
            {
                if (token.IsCancel()) return false;
                if (TryGetLoadedQuery(name, out _)) return true;
                return await AcquireQuery(name, token) != null;
            }
        }

        /// <summary>
        /// 获取已加载的查询对象（优先临时表，其次离线表），不触发任何异步/磁盘加载。
        /// </summary>
        public bool TryGetLoadedQuery(string name, out DtNavMeshQuery query)
        {
            query = null;
            if (string.IsNullOrEmpty(name)) return false;
            if (temporaryNavmeshs.TryGetValue(name, out TemporaryNavMesh tmp))
            {
                query = tmp.Query;
                return true;
            }
            return navmeshs.TryGetValue(name, out query);
        }

        /// <summary>
        /// 同步查询最近多边形。坐标按 Find 的 (-x, y, z) 规则转换。
        /// 返回 false 表示未加载或查询失败，调用方应视为“无地形数据”。
        /// </summary>
        public bool TryGetNearestPoly(string name, Vector3 position, out long polyRef,
            out Vector3 nearest, out bool overPoly)
        {
            polyRef = 0;
            nearest = position;
            overPoly = false;
            if (!TryGetLoadedQuery(name, out DtNavMeshQuery query)) return false;

            RcVec3f center = new(-position.x, position.y, position.z);
            DtStatus status = query.FindNearestPoly(center, extents, filter, out polyRef,
                out RcVec3f nearestPt, out overPoly);
            if (status.Failed() || polyRef == 0) return false;

            nearest = new Vector3(-nearestPt.x, nearestPt.y, nearestPt.z);
            return true;
        }

        /// <summary>
        /// 同步获取多边形表面高度（沿多边形垂直投影）。
        /// </summary>
        public bool TryGetPolyHeight(string name, long polyRef, Vector3 position, out float height)
        {
            height = 0f;
            if (polyRef == 0 || !TryGetLoadedQuery(name, out DtNavMeshQuery query)) return false;
            RcVec3f pos = new(-position.x, position.y, position.z);
            return !query.GetPolyHeight(polyRef, pos, out height).Failed();
        }

        /// <summary>
        /// 同步获取多边形边界上离指定点最近的点（用于 BoundaryDistance 估算）。
        /// </summary>
        public bool TryGetPolyBoundary(string name, long polyRef, Vector3 position,
            out Vector3 boundary)
        {
            boundary = position;
            if (polyRef == 0 || !TryGetLoadedQuery(name, out DtNavMeshQuery query)) return false;
            RcVec3f pos = new(-position.x, position.y, position.z);
            if (query.ClosestPointOnPolyBoundary(polyRef, pos, out RcVec3f boundaryPt).Failed())
                return false;
            boundary = new Vector3(-boundaryPt.x, boundaryPt.y, boundaryPt.z);
            return true;
        }

        private async ETTask<DtNavMeshQuery> Load(string name)
        {
            byte[] buffer = (await ResourcesManager.Instance.LoadAsync<TextAsset>(name))?.bytes;
            if (buffer==null || buffer.Length == 0)
            {
                Log.Error($"no nav data: {name}");
                return null;
            }

            DtMeshSetReader reader = new DtMeshSetReader();
            using MemoryStream ms = new MemoryStream(buffer);
            using BinaryReader br = new BinaryReader(ms);
            var navMesh = reader.Read32Bit(br, 6); // cpp recast导出来的要用Read32Bit读取，DotRecast导出来的还没试过
            
            if (navMesh == null)
            {
                Log.Error($"寻路| Find 失败 pathfinding ptr is zero: {name}");
                return null;
            }
                
            var query = new DtNavMeshQuery(navMesh);
            navmeshs.Add(name, query);
            return query;
        }

        /// <summary>
        /// 获取查询对象：优先临时表；同名后台构建进行中则等待其完成后复查临时表；
        /// 仍无则回落离线表，最后才尝试 <see cref="Load"/>。
        /// </summary>
        private async ETTask<DtNavMeshQuery> AcquireQuery(string name, ETCancellationToken token)
        {
            if (temporaryNavmeshs.TryGetValue(name, out TemporaryNavMesh tmp))
            {
                return tmp.Query;
            }

            ETTask<bool> pending = null;
            lock (buildLock)
            {
                if (pendingBuilds.TryGetValue(name, out PendingTemporaryBuild job))
                {
                    pending = job.Task;
                }
            }
            if (pending != null)
            {
                bool ok = await pending;
                // 构建失败或已取消，不再回落 Load（避免 no nav data 误报）。
                if (token.IsCancel() || !ok) return null;
                if (temporaryNavmeshs.TryGetValue(name, out tmp))
                {
                    return tmp.Query;
                }
            }

            if (navmeshs.TryGetValue(name, out DtNavMeshQuery query))
            {
                return query;
            }
            query = await Load(name);
            // Load 结果已落全局缓存，取消仅跳过本次查询，不中途打断加载。
            if (token.IsCancel()) return null;
            if (query == null)
            {
                Log.Error($"寻路| Find 失败 pathfinding ptr is zero: {name}");
            }
            return query;
        }
        
        public async ETTask<bool> Find(string name, Vector3 start, Vector3 target, List<Vector3> result, ETCancellationToken token = null)
        {
            if (token.IsCancel())
            {
                return false;
            }
            using(await CoroutineLockManager.Instance.Wait(CoroutineLockType.PathQuery, name.GetHashCode()))
            {
                if (token.IsCancel())
                {
                    return false;
                }
                DtNavMeshQuery query = await AcquireQuery(name, token);
                if (query == null)
                {
                    return false;
                }
            
                RcVec3f startPos = new(-start.x, start.y, start.z);
                RcVec3f endPos = new(-target.x, target.y, target.z);

                long startRef;
                long endRef;
                RcVec3f startPt;
                RcVec3f endPt;
            
                query.FindNearestPoly(startPos, extents, filter, out startRef, out startPt, out _);
                query.FindNearestPoly(endPos, extents, filter, out endRef, out endPt, out _);
<<<<<<< HEAD
            
=======

>>>>>>> 5184d65d (新增地形求解器)
                polys.Clear();
                query.FindPath(startRef, endRef, startPt, endPt, filter, ref polys, new DtFindPathOption(0, float.MaxValue));

                if (0 >= polys.Count)
                {
                    return true;
                }
            
                // In case of partial path, make sure the end point is clamped to the last polygon.
                RcVec3f epos = RcVec3f.Of(endPt.x, endPt.y, endPt.z);
                if (polys[^1] != endRef)
                {
                    DtStatus dtStatus = query.ClosestPointOnPoly(polys[^1], endPt, out RcVec3f closest, out bool _);
                    if (dtStatus.Succeeded())
                    {
                        epos = closest;
                    }
                }

                straightPath.Clear();
                query.FindStraightPath(startPt, epos, polys, ref straightPath, MAX_POLYS, DtNavMeshQuery.DT_STRAIGHTPATH_ALL_CROSSINGS);
                // 防御性检查：仅在同步段开始前被取消的话不再写回 result（防止写入已回收的 ListComponent）。
                if (token.IsCancel())
                {
                    return false;
                }

                for (int i = 0; i < straightPath.Count; ++i)
                {
                    RcVec3f pos = straightPath[i].pos;
                    result.Add(new Vector3(-pos.x, pos.y, pos.z));
                }
                return true;
            }
        }

        /// <summary>
        /// 用世界坐标三角形数据临时构建一套 NavMesh，构建成功后可经 <see cref="Find"/> 查询，
        /// 使用完毕后调用 <see cref="DestroyTemporary"/> 释放。同步构建，仅建议低频调用。
        /// 顶点为 Unity 世界坐标轴序，内部自动翻转适配 Recast。
        /// </summary>
        public bool BuildTemporary(string name, List<Vector3> vertices, List<int> triangles)
        {
            if (string.IsNullOrEmpty(name) || vertices == null || triangles == null ||
                vertices.Count == 0 || triangles.Count == 0)
            {
                Log.Error($"寻路| 临时构建失败，参数不合法: {name}");
                return false;
            }
            if (temporaryNavmeshs.ContainsKey(name))
            {
                Log.Error($"寻路| 临时构建失败，同名已存在，请先 DestroyTemporary: {name}");
                return false;
            }

            TemporaryNavMesh nav = BuildTemporaryInternal(ToRecastVerts(vertices), ToRecastTris(triangles), out string error);
            if (nav == null)
            {
                Log.Error($"寻路| 临时构建失败: {name} ({error})");
                return false;
            }

            temporaryNavmeshs[name] = nav;
            return true;
        }

        /// <summary>
        /// 异步构建临时 NavMesh：输入数据在主线程复制后，Recast 构建在后台线程执行，
        /// 完成结果经主线程 <see cref="Update"/> 落库，避免阻塞主线程。与 <see cref="BuildTemporary"/>
        /// 共用同名唯一约束。
        /// </summary>
        public async ETTask<bool> BuildTemporaryAsync(string name, List<Vector3> vertices, List<int> triangles)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return BuildTemporary(name, vertices, triangles);
#else
            if (string.IsNullOrEmpty(name) || vertices == null || triangles == null ||
                vertices.Count == 0 || triangles.Count == 0)
            {
                Log.Error($"寻路| 临时构建失败，参数不合法: {name}");
                return false;
            }
            if (temporaryNavmeshs.ContainsKey(name))
            {
                Log.Error($"寻路| 临时构建失败，同名已存在，请先 DestroyTemporary: {name}");
                return false;
            }

            while (true)
            {
                ETTask<bool> existingTask = null;
                lock (buildLock)
                {
                    if (pendingBuilds.TryGetValue(name, out PendingTemporaryBuild existing))
                    {
                        // 同名后台构建进行中：等其结束后复查，可用则复用，否则用本次数据重建。
                        existingTask = existing.Task;
                    }
                }

                if (existingTask != null)
                {
                    await existingTask;
                    if (temporaryNavmeshs.ContainsKey(name))
                    {
                        return true;
                    }
                    continue;
                }

                PendingTemporaryBuild job = new PendingTemporaryBuild(
                    name, ToRecastVerts(vertices), ToRecastTris(triangles), ETTask<bool>.Create());
                lock (buildLock)
                {
                    // 极端并发下注册瞬间被抢占：放弃本次 job，改等已登记任务。
                    if (pendingBuilds.TryGetValue(name, out PendingTemporaryBuild existing))
                    {
                        existingTask = existing.Task;
                    }
                    else
                    {
                        pendingBuilds.Add(name, job);
                    }
                }

                if (existingTask != null)
                {
                    await existingTask;
                    if (temporaryNavmeshs.ContainsKey(name))
                    {
                        return true;
                    }
                    continue;
                }

                ThreadPool.QueueUserWorkItem(RunTemporaryBuildWorker, job);
                return await job.Task;
            }
#endif
        }

        /// <summary>
        /// 后台线程执行 Recast 构建，完成后将结果并入主线程处理队列。
        /// </summary>
        private void RunTemporaryBuildWorker(object state)
        {
            var job = (PendingTemporaryBuild)state;
            TemporaryNavMesh result = BuildTemporaryInternal(job.Vertices, job.Triangles, out string error);
            lock (buildLock)
            {
                job.NavMesh = result;
                job.Error = error;
                completedBuilds.Add(job);
            }
        }

        /// <summary>
        /// 主线程每帧收割后台构建结果，落库查询表并唤醒异步等待方。
        /// </summary>
        public void Update()
        {
            PendingTemporaryBuild[] jobs;
            lock (buildLock)
            {
                if (completedBuilds.Count == 0) return;
                jobs = completedBuilds.ToArray();
                completedBuilds.Clear();
            }

            for (int i = 0; i < jobs.Length; i++)
            {
                PendingTemporaryBuild job = jobs[i];
                lock (buildLock)
                {
                    pendingBuilds.Remove(job.Name);
                }

                if (canceledBuilds.Remove(job.Name))
                {
                    job.Task.SetResult(false);
                    continue;
                }

                if (job.NavMesh == null)
                {
                    Log.Error($"寻路| 临时构建失败: {job.Name} ({job.Error})");
                    job.Task.SetResult(false);
                    continue;
                }

                temporaryNavmeshs[job.Name] = job.NavMesh;
                job.Task.SetResult(true);
            }
        }

        /// <summary>
        /// Recast 纯计算：由浮点顶点/索引构建临时 NavMesh 查询对象。仅触碰只读配置，
        /// 可在线程间安全调用。
        /// </summary>
        private static TemporaryNavMesh BuildTemporaryInternal(List<float> verts, List<int> tris, out string error)
        {
            error = null;
            try
            {
                IInputGeomProvider geom = new SimpleInputGeomProvider(verts, tris);
                var builderCfg = new RecastBuilderConfig(
                    TempBuildConfig, geom.GetMeshBoundsMin(), geom.GetMeshBoundsMax());
                RecastBuilderResult result = new RecastBuilder().Build(geom, builderCfg);
                RcPolyMesh pm = result?.GetMesh();
                if (pm == null || pm.npolys == 0)
                {
                    error = $"未生成可行走多边形 (verts={verts.Count / 3}, tris={tris.Count / 3})";
                    return null;
                }

                for (int i = 0; i < pm.npolys; ++i)
                {
                    pm.flags[i] = 1;
                }

                var option = new DtNavMeshCreateParams
                {
                    verts = pm.verts,
                    vertCount = pm.nverts,
                    polys = pm.polys,
                    polyAreas = pm.areas,
                    polyFlags = pm.flags,
                    polyCount = pm.npolys,
                    nvp = pm.nvp,
                    bmin = pm.bmin,
                    bmax = pm.bmax,
                    walkableHeight = TempBuildConfig.WalkableHeightWorld,
                    walkableRadius = TempBuildConfig.WalkableRadiusWorld,
                    walkableClimb = TempBuildConfig.WalkableClimbWorld,
                    cs = TempBuildConfig.Cs,
                    ch = TempBuildConfig.Ch,
                    buildBvTree = true,
                    offMeshConCount = 0,
                    offMeshConVerts = Array.Empty<float>(),
                    offMeshConRad = Array.Empty<float>(),
                    offMeshConDir = Array.Empty<int>(),
                    offMeshConAreas = Array.Empty<int>(),
                    offMeshConFlags = Array.Empty<int>(),
                    offMeshConUserID = Array.Empty<int>(),
                };
                RcPolyMeshDetail dm = result.GetMeshDetail();
                if (dm != null)
                {
                    option.detailMeshes = dm.meshes;
                    option.detailVerts = dm.verts;
                    option.detailVertsCount = dm.nverts;
                    option.detailTris = dm.tris;
                    option.detailTriCount = dm.ntris;
                }

                DtMeshData meshData = DtNavMeshBuilder.CreateNavMeshData(option);
                if (meshData == null)
                {
                    error = "CreateNavMeshData 返回空";
                    return null;
                }

                return new TemporaryNavMesh(new DtNavMeshQuery(new DtNavMesh(meshData, 6, 0)));
            }
            catch (Exception e)
            {
                error = e.ToString();
                return null;
            }
        }

        /// <summary>
        /// Unity 世界坐标顶点转为 Recast 轴序浮点扁平数组（-x, y, z）。
        /// </summary>
        private static List<float> ToRecastVerts(List<Vector3> vertices)
        {
            var verts = new List<float>(vertices.Count * 3);
            for (int i = 0; i < vertices.Count; i++)
            {
                Vector3 v = vertices[i];
                verts.Add(-v.x);
                verts.Add(v.y);
                verts.Add(v.z);
            }
            return verts;
        }

        /// <summary>
        /// Unity 逆时针索引转为 Recast 顺时针绕序。
        /// </summary>
        private static List<int> ToRecastTris(List<int> triangles)
        {
            var tris = new List<int>(triangles.Count);
            for (int i = 0; i + 2 < triangles.Count; i += 3)
            {
                tris.Add(triangles[i]);
                tris.Add(triangles[i + 2]);
                tris.Add(triangles[i + 1]);
            }
            return tris;
        }

        /// <summary>
        /// 销毁指定临时 NavMesh（含取消后台构建中的同名任务）。无显式释放接口，移除引用后交由 GC 回收。
        /// </summary>
        public void DestroyTemporary(string name)
        {
            if (string.IsNullOrEmpty(name)) return;
            temporaryNavmeshs.Remove(name);
            lock (buildLock)
            {
                if (pendingBuilds.ContainsKey(name))
                {
                    canceledBuilds.Add(name);
                }
            }
        }

        /// <summary>
        /// 销毁全部临时 NavMesh（含取消后台构建）。
        /// </summary>
        public void DestroyAllTemporary()
        {
            temporaryNavmeshs.Clear();
            lock (buildLock)
            {
                foreach (string name in pendingBuilds.Keys)
                {
                    canceledBuilds.Add(name);
                }
            }
        }
    }
}