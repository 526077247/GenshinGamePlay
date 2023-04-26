using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class AIPathFindingKnowledge: IDisposable
    {
        public PathFindingType Type;
        public string NavMeshAgentName;
        public DictionaryComponent<long, PathQueryTask> QueryTasks;

        public static AIPathFindingKnowledge Create(ConfigAIBeta config)
        {
            AIPathFindingKnowledge res = ObjectPool.Instance.Fetch<AIPathFindingKnowledge>();
            res.Type = config.Path.Type;
            res.NavMeshAgentName = config.Path.NavMeshAgentName;
            res.QueryTasks = DictionaryComponent<long, PathQueryTask>.Create();
            return res;
        }
        public void Dispose()
        {
            Type = default;
            NavMeshAgentName = null;
            foreach (var item in QueryTasks)
            {
                item.Value.Dispose();
            }
            QueryTasks.Dispose();
            QueryTasks = null;
        }

        public PathQueryTask CreatePathQueryTask(Vector3 start, Vector3 destination, NavMeshUseType type = NavMeshUseType.Auto)
        {
            var res = PathQueryTask.Create(start, destination,this,type);
            QueryTasks.Add(res.Id,res);
            return res;
        }

        public void ReleasePathQueryTask(long id)
        {
            if (QueryTasks.TryGetValue(id, out var res))
            {
                res.Dispose();
                QueryTasks.Remove(res.Id);
            }
        }
    }
}