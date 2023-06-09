namespace TaoTie
{
    public class SkillPrepareInfo: MoveInfoBase
    {
        private long timeoutTick;
        public override void Enter(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIManager aiManager)
        {
            base.Enter(taskHandler, aiKnowledge, aiManager);
            
            var skillConfig = aiKnowledge.ActionControlState.Skill.Config;
            if(!skillConfig.EnableSkillPrepare) return;
            AILocomotionHandler.ParamGoTo param = new AILocomotionHandler.ParamGoTo
            {
                targetPosition = aiKnowledge.TargetKnowledge.TargetPosition,
                speedLevel = skillConfig.SkillPrepareSpeedLevel,
            };
            taskHandler.CreateGoToTask(param);
            timeoutTick = skillConfig.SkillPrepareTimeout + GameTimerManager.Instance.GetTimeNow();
        }

        public override void UpdateInternal(AILocomotionHandler taskHandler, AIKnowledge aiKnowledge, AIComponent lcai, AIManager aiManager)
        {
            if (GameTimerManager.Instance.GetTimeNow() > timeoutTick)
            {
                if (taskHandler.currentState == LocoTaskState.Running)
                    taskHandler.currentState = LocoTaskState.Interrupted;
                return;
            }
            if (aiKnowledge.ActionControlState.Status == SkillStatus.Preparing)
            {
                var targetDistance = aiKnowledge.TargetKnowledge.TargetDistanceXZ;
                var castRangeMax = aiKnowledge.ActionControlState.Skill.Config.CastCondition.CastRangeMax;

                if (targetDistance < castRangeMax)
                {
                    if (taskHandler.currentState == LocoTaskState.Running)
                        taskHandler.currentState = LocoTaskState.Interrupted;
                }
            }
        }

        public override void Dispose()
        {
            timeoutTick = 0;
        }
    }
}