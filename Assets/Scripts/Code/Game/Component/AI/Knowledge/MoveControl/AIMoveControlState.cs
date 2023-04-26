using System;

namespace TaoTie
{
    public class AIMoveControlState : IDisposable
    {
        private MoveInfoBase[] moveInfoGroup;
        private MoveInfoBase curMoveInfo;

        public FacingMoveInfo FacingMoveInfo => moveInfoGroup[(int) MoveDecision.FacingMove] as FacingMoveInfo;
        public FleeInfo FleeInfo => moveInfoGroup[(int) MoveDecision.Flee] as FleeInfo;
        public WanderInfo WanderInfo =>  moveInfoGroup[(int) MoveDecision.Wander] as WanderInfo;
        public MeleeChargeInfo MeleeCharge =>  moveInfoGroup[(int) MoveDecision.MeleeCharge] as MeleeChargeInfo;
        public CombatFollowMoveInfo CombatFollowMove =>  moveInfoGroup[(int) MoveDecision.CombatFollowMove] as CombatFollowMoveInfo;
        public static AIMoveControlState Create()
        {
            AIMoveControlState res = ObjectPool.Instance.Fetch<AIMoveControlState>();
            res.moveInfoGroup = new MoveInfoBase[(int) MoveDecision.Max];
            res.moveInfoGroup[(int) MoveDecision.FacingMove] = FacingMoveInfo.Create();
            res.moveInfoGroup[(int) MoveDecision.Flee] = FleeInfo.Create();
            res.moveInfoGroup[(int) MoveDecision.Wander] = WanderInfo.Create();
            res.moveInfoGroup[(int) MoveDecision.MeleeCharge] = MeleeChargeInfo.Create();
            res.moveInfoGroup[(int) MoveDecision.CombatFollowMove] = CombatFollowMoveInfo.Create();
            return res;
        }

        public void Dispose()
        {
            for (int i = 0; i < moveInfoGroup.Length; i++)
            {
                if (moveInfoGroup[i] != null)
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
            else
            {
                taskHandler.UpdateMotionFlag(AIMoveSpeedLevel.Idle);
            }
        }

        public void UpdateMoveInfo(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIComponent lcai,
            AIManager aiManager)
        {
            if (curMoveInfo == null)
                return;

            curMoveInfo.UpdateMoveInfo(taskHandler, aiKnowledge, lcai,aiManager);
        }
    }
}