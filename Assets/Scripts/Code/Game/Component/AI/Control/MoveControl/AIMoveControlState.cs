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
        public SkillPrepareInfo SkillPrepareInfo => moveInfoGroup[(int) MoveDecision.SkillPrepare] as SkillPrepareInfo;
        public StandStillInfo StandStillInfo => moveInfoGroup[(int) MoveDecision.StandStill] as StandStillInfo;
        public static AIMoveControlState Create()
        {
            AIMoveControlState res = ObjectPool.Instance.Fetch<AIMoveControlState>();
            res.moveInfoGroup = new MoveInfoBase[(int) MoveDecision.Max];
            // 已有
            res.moveInfoGroup[(int) MoveDecision.FacingMove] = FacingMoveInfo.Create();
            res.moveInfoGroup[(int) MoveDecision.Flee] = FleeInfo.Create();
            res.moveInfoGroup[(int) MoveDecision.Wander] = WanderInfo.Create();
            res.moveInfoGroup[(int) MoveDecision.MeleeCharge] = MeleeChargeInfo.Create();
            res.moveInfoGroup[(int) MoveDecision.CombatFollowMove] = CombatFollowMoveInfo.Create();
            res.moveInfoGroup[(int) MoveDecision.SkillPrepare] = SkillPrepareInfo.Create();
            res.moveInfoGroup[(int) MoveDecision.StandStill] = StandStillInfo.Create();
            // 非战斗移动
            res.moveInfoGroup[(int) MoveDecision.ReturnToBorn] = ReturnToBornInfo.Create();
            res.moveInfoGroup[(int) MoveDecision.Investigate] = InvestigateInfo.Create();
            res.moveInfoGroup[(int) MoveDecision.ScriptedMoveTo] = ScriptedMoveToInfo.Create();
            res.moveInfoGroup[(int) MoveDecision.Extraction] = ExtractionInfo.Create();
            res.moveInfoGroup[(int) MoveDecision.Landing] = LandingInfo.Create();
            // 战斗环绕
            res.moveInfoGroup[(int) MoveDecision.Surround] = SurroundInfo.Create();
            res.moveInfoGroup[(int) MoveDecision.BirdCircling] = BirdCirclingInfo.Create();
            // 战术移动
            res.moveInfoGroup[(int) MoveDecision.FindBack] = FindBackInfo.Create();
            res.moveInfoGroup[(int) MoveDecision.CrabMove] = CrabMoveInfo.Create();
            res.moveInfoGroup[(int) MoveDecision.CombatFixedMove] = CombatFixedMoveInfo.Create();
            res.moveInfoGroup[(int) MoveDecision.SpacialChase] = SpacialChaseInfo.Create();
            res.moveInfoGroup[(int) MoveDecision.SpacialProbe] = SpacialProbeInfo.Create();
            res.moveInfoGroup[(int) MoveDecision.SpacialAdjust] = SpacialAdjustInfo.Create();
            // 巡逻与行动点
            res.moveInfoGroup[(int) MoveDecision.PatrolFollow] = PatrolFollowInfo.Create();
            res.moveInfoGroup[(int) MoveDecision.ReactActionPoint] = ReactActionPointInfo.Create();
            // 复用同类型
            res.moveInfoGroup[(int) MoveDecision.FollowScriptedPath] = PatrolFollowInfo.Create();
            res.moveInfoGroup[(int) MoveDecision.FollowServerRoute] = PatrolFollowInfo.Create();
            res.moveInfoGroup[(int) MoveDecision.BrownianMove] = WanderInfo.Create();
            res.moveInfoGroup[(int) MoveDecision.AutoPlayerSkillPrepare] = SkillPrepareInfo.Create();
            res.moveInfoGroup[(int) MoveDecision.AutoPlayerFollowTarget] = CombatFollowMoveInfo.Create();
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
                taskHandler.UpdateMotionFlag(MotionFlag.Idle);
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