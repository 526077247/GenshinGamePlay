namespace TaoTie
{
    public class SkillPrepareInfo: MoveInfoBase
    {
        private long timeoutTick;

        public static SkillPrepareInfo Create()
        {
            return ObjectPool.Instance.Fetch<SkillPrepareInfo>();
        }
        public override void Enter(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIManager aiManager)
        {
            base.Enter(taskHandler, aiKnowledge, aiManager);
            Check(taskHandler, aiKnowledge);
        }

        private void Check(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge)
        {
            var skillConfig = aiKnowledge.ActionControlState.Skill.Config;
            if(!skillConfig.EnableSkillPrepare) return;
            if (aiKnowledge.TargetKnowledge.TargetDistanceXZ > skillConfig.CastCondition.CastRangeMax)
            {
                AILocomotionHandler.ParamFacingMove param = new AILocomotionHandler.ParamFacingMove
                {
                    Anchor = aiKnowledge.TargetKnowledge.TargetEntity,
                    SpeedLevel = skillConfig.SkillPrepareSpeedLevel,
                    Duration = 1000,
                    MovingDirection = MotionDirection.Forward
                };
                taskHandler.CreateFacingMoveTask(param);
            }
            else if (aiKnowledge.TargetKnowledge.TargetDistanceXZ < skillConfig.CastCondition.CastRangeMin)
            {
                AILocomotionHandler.ParamFacingMove param = new AILocomotionHandler.ParamFacingMove
                {
                    Anchor = aiKnowledge.TargetKnowledge.TargetEntity,
                    SpeedLevel = skillConfig.SkillPrepareSpeedLevel,
                    Duration = 1000,
                    MovingDirection = MotionDirection.Backward
                };

                taskHandler.CreateFacingMoveTask(param);
            }
            timeoutTick = skillConfig.SkillPrepareTimeout + GameTimerManager.Instance.GetTimeNow();
        } 

        public override void UpdateInternal(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIComponent lcai, AIManager aiManager)
        {
            if (GameTimerManager.Instance.GetTimeNow() > timeoutTick)
            {
                Check(taskHandler,aiKnowledge);
            }
        }

        public override void Dispose()
        {
            timeoutTick = 0;
            ObjectPool.Instance.Recycle(this);
        }
    }
}