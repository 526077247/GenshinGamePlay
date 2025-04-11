using System;
using System.Collections.Generic;
using System.IO;
using DotRecast.Core;
using DotRecast.Detour;
using DotRecast.Detour.Io;
using UnityEngine;

namespace TaoTie
{
    public class NavmeshSystem:IManager
    {
        const int MAX_POLYS = 256;
        
        const int FindRandomNavPosMaxRadius = 15000;  // 随机找寻路点的最大半径
        
        public static NavmeshSystem Instance;
        private readonly Dictionary<string, DtNavMeshQuery> navmeshs = new Dictionary<string, DtNavMeshQuery>();
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
        
        public async ETTask<bool> Find(string name, Vector3 start, Vector3 target, List<Vector3> result)
        {
            using(await CoroutineLockManager.Instance.Wait(CoroutineLockType.PathQuery, name.GetHashCode()))
            {
                if (!navmeshs.TryGetValue(name,out var query))
                {
                    query = await Load(name);
                    if (query == null)
                    {
                        Log.Error($"寻路| Find 失败 pathfinding ptr is zero: {name}");
                        return false;
                    }
                }
            
                RcVec3f startPos = new(-start.x, start.y, start.z);
                RcVec3f endPos = new(-target.x, target.y, target.z);

                long startRef;
                long endRef;
                RcVec3f startPt;
                RcVec3f endPt;
            
                query.FindNearestPoly(startPos, extents, filter, out startRef, out startPt, out _);
                query.FindNearestPoly(endPos, extents, filter, out endRef, out endPt, out _);
            
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

                query.FindStraightPath(startPt, epos, polys, ref straightPath, MAX_POLYS, DtNavMeshQuery.DT_STRAIGHTPATH_ALL_CROSSINGS);

                for (int i = 0; i < straightPath.Count; ++i)
                {
                    RcVec3f pos = straightPath[i].pos;
                    result.Add(new Vector3(-pos.x, pos.y, pos.z));
                }
                return true;
            }
        }
    }
}