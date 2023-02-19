using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 寻路
    /// </summary>
    public class AIPathfindingUpdater: BrainModuleBase
    {

        protected override void UpdateMainThreadInternal()
        {
            base.UpdateMainThreadInternal();
            if(knowledge.targetKnowledge.targetType == AITargetType.InvalidTarget) return;
            if (knowledge.targetKnowledge.hasPath == AITargetHasPathType.Invalid)
            {
                Vector3 aimPos;
                if (knowledge.targetKnowledge.targetType == AITargetType.EntityTarget)
                {
                    aimPos= knowledge.targetKnowledge.targetEntity.Position;
                }
                else
                {
                    aimPos= knowledge.targetKnowledge.targetPosition;
                }
                if (knowledge.pathFindingKnowledge.Route != null)
                {
                    knowledge.pathFindingKnowledge.Route.Dispose();
                    knowledge.pathFindingKnowledge.Route = null;
                }
                if (knowledge.pathFindingKnowledge.Type == PathFindingType.Link)
                {
                    //直接冲过去
                    knowledge.pathFindingKnowledge.Route = ListComponent<Vector3>.Create();
                    knowledge.pathFindingKnowledge.Route.Add(knowledge.currentPos);
                    knowledge.pathFindingKnowledge.Route.Add(aimPos);
                    knowledge.targetKnowledge.hasPath = AITargetHasPathType.Success;
                }
                else if (knowledge.pathFindingKnowledge.Type == PathFindingType.NavMesh)
                {
                    //todo:接入navmesh
                }
            }
        }
    }
}