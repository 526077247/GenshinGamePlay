using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 寻路
    /// </summary>
    public class AIPathfindingUpdater : BrainModuleBase
    {

        protected override void UpdateMainThreadInternal()
        {
            base.UpdateMainThreadInternal();
            foreach (var item in knowledge.pathFindingKnowledge.QueryTasks)
            {
                var task = item.Value;
                if (task.status != QueryStatus.Pending) continue;
                if (task.type == NavMeshUseType.NotUse ||
                    (task.type == NavMeshUseType.Auto && knowledge.pathFindingKnowledge.Type == PathFindingType.Link))
                {
                    //直接冲过去
                    task.corners.Add(task.start);
                    task.corners.Add(task.destination);
                    task.status = QueryStatus.Success;
                }
                else if (task.type == NavMeshUseType.ForceUse ||
                         (task.type == NavMeshUseType.Auto &&
                          knowledge.pathFindingKnowledge.Type == PathFindingType.NavMesh))
                {
                    //todo:接入navmesh
                    task.status = QueryStatus.Fail;
                }
            }
        }
    }
}