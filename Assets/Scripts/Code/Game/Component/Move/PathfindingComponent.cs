using System;
using System.Collections.Generic;
using System.IO;
using DotRecast.Core;
using DotRecast.Detour;
using DotRecast.Detour.Io;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 同一块地图可能有多种寻路数据，玩家可以随时切换，怪物也可能跟玩家的寻路不一样，寻路组件应该挂在Entity上
    /// </summary>
    public class PathfindingComponent: Component, IComponent<string>
    {
        public const int MAX_POLYS = 256;
        
        public const int FindRandomNavPosMaxRadius = 15000;  // 随机找寻路点的最大半径
        
        public RcVec3f extents = new(15, 10, 15);
        
        public string Name;
        
        public DtNavMesh navMesh;
        
        public List<long> polys = new(MAX_POLYS);

        public IDtQueryFilter filter;
        
        public List<StraightPathItem> straightPath = new();

        public DtNavMeshQuery query;
        
        #region IComponent

        public void Init(string name)
        {
            this.Name = name;
            byte[] buffer = NavmeshSystem.Instance.Get(name);
            
            DtMeshSetReader reader = new();
            using MemoryStream ms = new(buffer);
            using BinaryReader br = new(ms);
            this.navMesh = reader.Read32Bit(br, 6); // cpp recast导出来的要用Read32Bit读取，DotRecast导出来的还没试过
            
            if (this.navMesh == null)
            {
                throw new Exception($"nav load fail: {name}");
            }
            
            this.filter = new DtQueryDefaultFilter();
            this.query = new DtNavMeshQuery(this.navMesh);
        }

        public void Destroy()
        {
            this.Name = string.Empty;
            this.navMesh = null;
        }

        #endregion
        
        public void Find(Vector3 start, Vector3 target, List<Vector3> result)
        {
            if (navMesh == null)
            {
                throw new Exception($"寻路| Find 失败 pathfinding ptr is zero: {Name}");
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
                return;
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

            query.FindStraightPath(startPt, epos, polys, ref straightPath, PathfindingComponent.MAX_POLYS, DtNavMeshQuery.DT_STRAIGHTPATH_ALL_CROSSINGS);

            for (int i = 0; i < straightPath.Count; ++i)
            {
                RcVec3f pos = straightPath[i].pos;
                result.Add(new Vector3(-pos.x, pos.y, pos.z));
            }
        }
    }
}