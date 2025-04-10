using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 寻路
    /// </summary>
    public class AIPathfindingUpdater : BrainModuleBase
    {
        protected override void InitInternal()
        {
            base.InitInternal();
            knowledge.Entity.GetComponent<ORCAAgentComponent>()?.EnableRVO2(knowledge.PathFindingKnowledge.UseRVO2);
        }

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
                    task.Corners.Clear();
                    //直接冲过去
                    task.Corners.Add(task.Start);
                    task.Corners.Add(task.Destination);
                    task.Status = QueryStatus.Success;
                }
                else if (task.Type == NavMeshUseType.ForceUse ||
                         (task.Type == NavMeshUseType.Auto &&
                          knowledge.PathFindingKnowledge.Type == PathFindingType.NavMesh))
                {
                    task.Corners.Clear();
                    var pc = this.knowledge.Entity.GetComponent<PathfindingComponent>();
                    if (pc != null)
                    {
                        if (pc.Name != this.knowledge.PathFindingKnowledge.NavMeshAgentName)
                        {
                            this.knowledge.Entity.RemoveComponent(pc);
                            pc = null;
                        }
                    }
                    if (pc == null)
                    {
                        if(string.IsNullOrEmpty(this.knowledge.PathFindingKnowledge.NavMeshAgentName))
                        {
                            task.Status = QueryStatus.Fail;
                            Log.Error($"寻路失败，{this.knowledge.Entity.Id}没找到PathfindingComponent组件");
                            return;
                        }

                        pc = this.knowledge.Entity.AddComponent<PathfindingComponent, string>(this.knowledge
                            .PathFindingKnowledge.NavMeshAgentName);
                    }
                    FindNavmeshTask(pc, task).Coroutine();
                }
            }
        }
        private async ETTask FindNavmeshTask(PathfindingComponent pc, PathQueryTask task)
        {
            task.Status = QueryStatus.Querying;
            task.Status = await pc.Find(task.Start, task.Destination, task.Corners)?QueryStatus.Success:QueryStatus.Fail;
        }
    }
}