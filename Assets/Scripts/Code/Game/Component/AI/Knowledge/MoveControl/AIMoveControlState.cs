using System;

namespace TaoTie
{
    public class AIMoveControlState : IDisposable
    {
        public MoveInfoBase[] moveInfoGroup;
        public MoveInfoBase curMoveInfo;

        public FacingMoveInfo FacingMoveInfo => moveInfoGroup[(int) MoveDecision.FacingMove] as FacingMoveInfo;

        public static AIMoveControlState Create()
        {
            AIMoveControlState res = ObjectPool.Instance.Fetch<AIMoveControlState>();
            res.moveInfoGroup = new MoveInfoBase[(int) MoveDecision.Max];
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
        }

        public void UpdateMoveInfo(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIComponent lcai,
            AIManager aiManager)
        {
        }
    }
}