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
            foreach (var item in knowledge.PathFindingKnowledge.QueryTasks)
            {
                var task = item.Value;
                if (task.Status != QueryStatus.Pending) continue;
                if (task.Type == NavMeshUseType.NotUse ||
                    (task.Type == NavMeshUseType.Auto && knowledge.PathFindingKnowledge.Type == PathFindingType.Link))
                {
                    //直接冲过去
                    task.Corners.Add(task.Start);
                    task.Corners.Add(task.Destination);
                    task.Status = QueryStatus.Success;
                }
                else if (task.Type == NavMeshUseType.ForceUse ||
                         (task.Type == NavMeshUseType.Auto &&
                          knowledge.PathFindingKnowledge.Type == PathFindingType.NavMesh))
                {
                    //todo:接入navmesh
                    task.Status = QueryStatus.Fail;
                }
            }
        }
    }
}