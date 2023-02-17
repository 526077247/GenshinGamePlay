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
                if (knowledge.pathFindingKnowledge.route != null)
                {
                    knowledge.pathFindingKnowledge.route.Dispose();
                    knowledge.pathFindingKnowledge.route = null;
                }
                if (knowledge.pathFindingKnowledge.type == PathFindingType.Link)
                {
                    //直接冲过去
                    knowledge.pathFindingKnowledge.route = ListComponent<Vector3>.Create();
                    knowledge.pathFindingKnowledge.route.Add(knowledge.currentPos);
                    knowledge.pathFindingKnowledge.route.Add(aimPos);
                    knowledge.targetKnowledge.hasPath = AITargetHasPathType.Success;
                }
                else if (knowledge.pathFindingKnowledge.type == PathFindingType.NavMesh)
                {
                    //todo:
                }
            }
        }
    }
}