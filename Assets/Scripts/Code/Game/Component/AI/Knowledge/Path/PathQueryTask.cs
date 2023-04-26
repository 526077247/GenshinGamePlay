using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class PathQueryTask: IDisposable
    {
        public long Id;
        public ListComponent<Vector3> Corners;
        public Vector3 Start;
        public Vector3 Destination;
        public QueryStatus Status;
        public NavMeshUseType Type;
        private AIPathFindingKnowledge PathFindingKnowledge;

        public static PathQueryTask Create(Vector3 start,Vector3 destination,AIPathFindingKnowledge knowledge,NavMeshUseType type=NavMeshUseType.Auto)
        {
            PathQueryTask res = ObjectPool.Instance.Fetch<PathQueryTask>();
            res.PathFindingKnowledge = knowledge;
            res.Id = IdGenerater.Instance.GenerateId();
            res.Start = start;
            res.Destination = destination;
            res.Corners = ListComponent<Vector3>.Create();
            res.Status = QueryStatus.Pending;
            res.Type = type;
            return res;
        }

        public void Dispose()
        {
            if (PathFindingKnowledge != null)
            {
                var know = PathFindingKnowledge;
                PathFindingKnowledge = null;
                know.ReleasePathQueryTask(Id);
                Id = 0;
                Corners?.Dispose();
                Corners = null;
                Start = default;
                Destination = default;
                Status = QueryStatus.Inactive;
                Type = NavMeshUseType.Auto;
            }
        }
    }
}