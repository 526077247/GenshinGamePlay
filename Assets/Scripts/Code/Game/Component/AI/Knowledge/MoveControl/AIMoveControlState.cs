using System;

namespace TaoTie
{
    public class AIMoveControlState : IDisposable
    {
        public MoveInfoBase[] moveInfoGroup;
        public MoveInfoBase curMoveInfo;

        public FacingMoveInfo FacingMoveInfo => moveInfoGroup[(int) MoveDecision.FacingMove] as FacingMoveInfo;
        public FleeInfo FleeInfo => moveInfoGroup[(int) MoveDecision.Flee] as FleeInfo;

        public static AIMoveControlState Create()
        {
            AIMoveControlState res = ObjectPool.Instance.Fetch<AIMoveControlState>();
            res.moveInfoGroup = new MoveInfoBase[(int) MoveDecision.Max];
            res.moveInfoGroup[(int) MoveDecision.FacingMove] = FacingMoveInfo.Create();
            res.moveInfoGroup[(int) MoveDecision.Flee] = FleeInfo.Create();
            return res;
        }

        public void Dispose()
        {
            for (int i = 0; i < moveInfoGroup.Length; i++)
            {
                if(moveInfoGroup[i]!=null)
                    moveInfoGroup[i].Dispose();
            }

            moveInfoGroup = null;
            curMoveInfo = null;
        }

        public void Goto(MoveDecision newDecision, AILocomotionHandler taskHandler, AIKnowledge aiKnowledge,
            AIManager aiManager)
        {
            if (curMoveInfo != null)
                curMoveInfo.Leave(taskHandler, aiKnowledge, aiManager);
            curMoveInfo = moveInfoGroup[(int) newDecision];
            if (curMoveInfo != null)
            {
                curMoveInfo.Enter(taskHandler, aiKnowledge, aiManager);
            }
        }

        public void UpdateMoveInfo(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIComponent lcai,
            AIManager aiManager)
        {
        }
    }
}