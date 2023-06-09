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
            
            var skillConfig = aiKnowledge.ActionControlState.Skill.Config;
            if(!skillConfig.EnableSkillPrepare) return;
            if (aiKnowledge.TargetKnowledge.TargetDistanceXZ > skillConfig.CastCondition.CastRangeMax)
            {
                AILocomotionHandler.ParamGoTo param = new AILocomotionHandler.ParamGoTo
                {
                    targetPosition = aiKnowledge.TargetKnowledge.TargetPosition,
                    speedLevel = skillConfig.SkillPrepareSpeedLevel,
                };
                taskHandler.CreateGoToTask(param);
            }
            else if (aiKnowledge.TargetKnowledge.TargetDistanceXZ < skillConfig.CastCondition.CastRangeMin)
            {
                AILocomotionHandler.ParamFacingMove param = new AILocomotionHandler.ParamFacingMove
                {
                    anchor = aiKnowledge.TargetKnowledge.TargetEntity,
                    speedLevel = skillConfig.SkillPrepareSpeedLevel,
                    duration = 1000,
                    movingDirection = MotionDirection.Backward
                };

                taskHandler.CreateFacingMoveTask(param);
            }

            timeoutTick = skillConfig.SkillPrepareTimeout + GameTimerManager.Instance.GetTimeNow();
        }

        public override void UpdateInternal(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIComponent lcai, AIManager aiManager)
        {
            if (GameTimerManager.Instance.GetTimeNow() > timeoutTick)
            {
                if (taskHandler.currentState == LocoTaskState.Running)
                    taskHandler.currentState = LocoTaskState.Interrupted;
            }
        }

        public override void Dispose()
        {
            timeoutTick = 0;
        }
    }
}