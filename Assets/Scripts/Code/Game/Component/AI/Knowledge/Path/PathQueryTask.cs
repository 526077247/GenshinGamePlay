using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class PathQueryTask: IDisposable
    {
        public long id;
        public ListComponent<Vector3> corners;
        public Vector3 start;
        public Vector3 destination;
        public QueryStatus status;
        public NavMeshUseType type;
        private AIPathFindingKnowledge pathFindingKnowledge;

        public static PathQueryTask Create(Vector3 start,Vector3 destination,AIPathFindingKnowledge knowledge,NavMeshUseType type=NavMeshUseType.Auto)
        {
            PathQueryTask res = ObjectPool.Instance.Fetch<PathQueryTask>();
            res.pathFindingKnowledge = knowledge;
            res.id = IdGenerater.Instance.GenerateId();
            res.start = start;
            res.destination = destination;
            res.corners = ListComponent<Vector3>.Create();
            res.status = QueryStatus.Pending;
            res.type = type;
            return res;
        }

        public void Dispose()
        {
            if (pathFindingKnowledge != null)
            {
                var know = pathFindingKnowledge;
                pathFindingKnowledge = null;
                know.ReleasePathQueryTask(id);
                id = 0;
                corners?.Dispose();
                corners = null;
                start = default;
                destination = default;
                status = QueryStatus.Inactive;
                type = NavMeshUseType.Auto;
            }
        }
    }
}