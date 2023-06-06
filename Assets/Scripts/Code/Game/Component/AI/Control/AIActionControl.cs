using System.Collections.Generic;

namespace TaoTie
{
    /// <summary>
    /// 行动
    /// </summary>
    public class AIActionControl : AIBaseControl
    {
        private AIActionControlState actionState;
        private List<AISkillInfo> validCandidates;
        private FsmComponent fsm;

        protected override void InitInternal()
        {
            actionState = knowledge.ActionControlState;
            fsm = knowledge.Entity.GetComponent<FsmComponent>();
        }

        public void ExecuteAction(AIDecision decision)
        {
            if (actionState.Status == SkillStatus.Inactive)
            {
                if (decision.Act == ActDecision.OnAware)
                {
                    if (knowledge.SkillKnowledge.SkillsOnAware.AvailableSkills.Count > 0)
                    {
                        SelectSkill(knowledge.SkillKnowledge.SkillsOnAware.AvailableSkills[0]);
                        CastSkill();
                        actionState.Status = SkillStatus.Playing;
                    }
                }
                else if (decision.Act == ActDecision.OnAlert)
                {
                    if (knowledge.SkillKnowledge.SkillsOnAlert.AvailableSkills.Count > 0)
                    {
                        SelectSkill(knowledge.SkillKnowledge.SkillsOnAlert.AvailableSkills[0]);
                        CastSkill();
                        actionState.Status = SkillStatus.Playing;
                    }
                }
                else if (decision.Act == ActDecision.FreeSkill)
                {
                    if (knowledge.SkillKnowledge.SkillsFree.AvailableSkills.Count > 0)
                    {
                        SelectSkill(knowledge.SkillKnowledge.SkillsFree.AvailableSkills[0]);
                        CastSkill();
                        actionState.Status = SkillStatus.Playing;
                    }
                }
                else if (decision.Act == ActDecision.BuddySkill)
                {
                    if (knowledge.SkillKnowledge.SkillsCombatBuddy.AvailableSkills.Count > 0)
                    {
                        SelectSkill(knowledge.SkillKnowledge.SkillsCombatBuddy.AvailableSkills[0]);
                        CastSkill();
                        actionState.Status = SkillStatus.Playing;
                    }
                }
                else if (decision.Act == ActDecision.CombatSkill)
                {
                    if (knowledge.SkillKnowledge.SkillsCombat.AvailableSkills.Count > 0)
                    {
                        SelectSkill(knowledge.SkillKnowledge.SkillsCombat.AvailableSkills[0]);
                        CastSkill();
                        actionState.Status = SkillStatus.Playing;
                    }
                }
                else if (decision.Act == ActDecision.CombatSkillPrepare)
                {
                    if (knowledge.SkillKnowledge.SkillsCombat.AvailableSkills.Count > 0)
                    {
                        actionState.Status = SkillStatus.Preparing;
                        SelectSkill(knowledge.SkillKnowledge.SkillsCombat.AvailableSkills[0]);
                        OnSkillStart();
                    }
                }
            }

            if (actionState.Status == SkillStatus.Preparing)
            {
                var targetKnowledge = knowledge.TargetKnowledge;
                var currentSkill = actionState.Skill;

                float rangeMin = -1;
                float rangeMax = -1;
                if (currentSkill.Config.CastCondition != null)
                {
                    rangeMin = currentSkill.Config.CastCondition.SkillAnchorRangeMin;
                    rangeMax = currentSkill.Config.CastCondition.SkillAnchorRangeMax;
                }
                
                if (rangeMin < 0 && rangeMax < 0)
                {
                    actionState.Status = SkillStatus.Prepared;
                }
                else if (targetKnowledge.SkillAnchorDistance <= rangeMax &&
                         targetKnowledge.SkillAnchorDistance >= rangeMin)
                {
                    actionState.Status = SkillStatus.Prepared;
                }
                else
                {
                    OnSkillFail();
                }
            }
            
            if (actionState.Status == SkillStatus.Prepared)
            {
                if (decision.Act == ActDecision.CombatSkill)
                {
                    var targetPosition = knowledge.TargetKnowledge.TargetPosition;
                    if (knowledge.TargetKnowledge.TargetType == AITargetType.EntityTarget)
                    {
                        targetPosition = knowledge.TargetKnowledge.TargetEntity.Position;
                    }

                    CastSkill();
                    actionState.Status = SkillStatus.Playing;
                    
                    if (actionState.Skill.Config.FaceTarget)
                    {
                        knowledge.Mover.ForceLookAt(targetPosition);
                    }
                }
            }
            
            if (actionState.Status == SkillStatus.Playing)
            {
                var skillInfo = actionState.Skill;

                bool isFinished = true;

                if (skillInfo.Config.StateIds?.Length > 0)
                {
                    for (int i = 0; i < skillInfo.Config.StateIds.Length; i++)
                    {
                        if (fsm.DefaultFsm.CurrentStateName == skillInfo.Config.StateIds[i])
                        {
                            isFinished = false;
                            break;
                        }
                    }
                }
                var targetPosition = knowledge.TargetKnowledge.TargetPosition;
                if (knowledge.TargetKnowledge.TargetType == AITargetType.EntityTarget)
                {
                    targetPosition = knowledge.TargetKnowledge.TargetEntity.Position;
                }
                if (actionState.Skill.Config.FaceTarget)
                {
                    knowledge.Mover.ForceLookAt(targetPosition);
                }
                if (isFinished)
                {
                    OnSkillFinish();
                    actionState.Status = SkillStatus.Inactive;
                }
            }
        }

        private void SelectSkill(AISkillInfo skill)
        {
            actionState.Skill = skill;
        }

        private void CastSkill()
        {
            var now = GameTimerManager.Instance.GetTimeNow();
            var maic = knowledge.CombatComponent;
            var skillInfo = actionState.Skill;
            var skillKnowledge = knowledge.SkillKnowledge;

            maic.UseSkillImmediately(skillInfo.SkillId);

            if (skillInfo.Config.TriggerCDOnStart)
            {
                skillInfo.TriggerCD(now);
                skillKnowledge.SetSkillGroupCD(skillInfo.Config.SkillGroupCDID, now);
                if (skillInfo.Config.TriggerGCD)
                    knowledge.SkillKnowledge.SetGCD(now);
                if (skillInfo.Config.TriggerGCD)
                    knowledge.AIManager.SetSkillUsed(skillInfo.Config.PublicCDGroup);
            }
        }

        private void OnSkillStart()
        {
            var skillInfo = actionState.Skill;
            var targetKnowledge = knowledge.TargetKnowledge;

            var currentPosition = knowledge.CurrentPos;
            currentPosition.y = 0;

            var targetDistanceXZ = targetKnowledge.TargetDistanceXZ;

            var targetPos = currentPosition;

            if (skillInfo.Config.CastCondition!=null && targetDistanceXZ > skillInfo.Config.CastCondition.CastRangeMax)
            {
                var direction = (currentPosition - targetKnowledge.TargetPosition).normalized * skillInfo.Config.CastCondition.CastRangeMax;
                targetPos = targetKnowledge.TargetPosition + direction;
            }
            knowledge.TargetKnowledge.SkillAnchorPosition = targetPos;
        }

        private void OnSkillFinish()
        {
            var now = GameTimerManager.Instance.GetTimeNow();
            var skillInfo = actionState.Skill;
            var skillKnowledge = knowledge.SkillKnowledge;
            if (skillInfo == null)
                return;
            
            if (!skillInfo.Config.TriggerCDOnStart)
            {
                skillInfo.TriggerCD(now);
                skillKnowledge.SetSkillGroupCD(skillInfo.Config.SkillGroupCDID, now);
                if (skillInfo.Config.TriggerGCD)
                    knowledge.SkillKnowledge.SetGCD(now);
                if (skillInfo.Config.TriggerGCD)
                    knowledge.AIManager.SetSkillUsed(skillInfo.Config.PublicCDGroup);
            }


            actionState.Reset();
        }

        private void OnSkillFail(bool needTriggerCD = true)
        {
            var now = GameTimerManager.Instance.GetTimeNow();
            var skillInfo = actionState.Skill;
            var skillKnowledge = knowledge.SkillKnowledge;
            var maic = knowledge.CombatComponent;
            maic.ReleaseSkillImmediately();
            if (skillInfo != null)
            {
                if (needTriggerCD)
                {
                    skillInfo.TriggerCD(now);
                    skillKnowledge.SetSkillGroupCD(skillInfo.Config.SkillGroupCDID, now);
                    if (skillInfo.Config.TriggerGCD)
                        knowledge.SkillKnowledge.SetGCD(now);
                    if (skillInfo.Config.TriggerGCD)
                        knowledge.AIManager.SetSkillUsed(skillInfo.Config.PublicCDGroup);
                }
            }

            actionState.Reset();
        }
    }
}