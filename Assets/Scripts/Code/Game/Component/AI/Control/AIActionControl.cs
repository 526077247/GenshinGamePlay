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
        private AIComponent component;

        protected override void InitInternal()
        {
            base.InitInternal();
            actionState = aiKnowledge.ActionControlState;
            component = aiKnowledge.AiOwnerEntity.GetComponent<AIComponent>();
        }

        public void ExecuteAction(AIDecision decision)
        {
            //Inactive Status: no skill stand by, select new skill
            if (actionState.Status == SkillStatus.Inactive)
            {
                if (decision.act == ActDecision.OnAware)
                {
                    if (aiKnowledge.SkillKnowledge.SkillsOnAware.AvailableSkills.Count > 0)
                    {
                        SelectSkill(aiKnowledge.SkillKnowledge.SkillsOnAware.AvailableSkills[0]);
                        CastSkill();
                        actionState.Status = SkillStatus.Playing;
                    }
                }
                else if (decision.act == ActDecision.OnAlert)
                {
                    if (aiKnowledge.SkillKnowledge.SkillsOnAlert.AvailableSkills.Count > 0)
                    {
                        SelectSkill(aiKnowledge.SkillKnowledge.SkillsOnAlert.AvailableSkills[0]);
                        CastSkill();
                        actionState.Status = SkillStatus.Playing;
                    }
                }
                else if (decision.act == ActDecision.FreeSkill)
                {
                    if (aiKnowledge.SkillKnowledge.SkillsFree.AvailableSkills.Count > 0)
                    {
                        SelectSkill(aiKnowledge.SkillKnowledge.SkillsFree.AvailableSkills[0]);
                        CastSkill();
                        actionState.Status = SkillStatus.Playing;
                    }
                }
                else if (decision.act == ActDecision.BuddySkill)
                {
                    if (aiKnowledge.SkillKnowledge.SkillsCombatBuddy.AvailableSkills.Count > 0)
                    {
                        SelectSkill(aiKnowledge.SkillKnowledge.SkillsCombatBuddy.AvailableSkills[0]);
                        CastSkill();
                        actionState.Status = SkillStatus.Playing;
                    }
                }
                else if (decision.act == ActDecision.CombatSkill)
                {
                    if (aiKnowledge.SkillKnowledge.SkillsCombat.AvailableSkills.Count > 0)
                    {
                        SelectSkill(aiKnowledge.SkillKnowledge.SkillsCombat.AvailableSkills[0]);
                        CastSkill();
                        actionState.Status = SkillStatus.Playing;
                    }
                }
                else if (decision.act == ActDecision.CombatSkillPrepare)
                {
                    if (aiKnowledge.SkillKnowledge.SkillsCombat.AvailableSkills.Count > 0)
                    {
                        actionState.Status = SkillStatus.Preparing;
                        SelectSkill(aiKnowledge.SkillKnowledge.SkillsCombat.AvailableSkills[0]);
                        OnSkillStart();
                    }
                }
            }

            //Preparing Status: in order to meet the castRange condition
            if (actionState.Status == SkillStatus.Preparing)
            {
                var targetKnowledge = aiKnowledge.TargetKnowledge;
                var currentSkill = actionState.Skill;

                var castRangeMin = currentSkill.Config.CastCondition.CastRangeMin;
                var castRangeMax = currentSkill.Config.CastCondition.CastRangeMax;

                var targetPosition = aiKnowledge.TargetKnowledge.TargetPosition;
                if (aiKnowledge.TargetKnowledge.TargetType == AITargetType.EntityTarget)
                {
                    targetPosition = aiKnowledge.TargetKnowledge.TargetEntity.Position;
                }

                var currentPosition = aiKnowledge.CurrentPos;

                targetPosition.y = 0;
                currentPosition.y = 0;


                if (castRangeMin < 0 && castRangeMax < 0)
                {
                    actionState.Status = SkillStatus.Prepared;
                }

                else if (targetKnowledge.SkillAnchorDistance < castRangeMax ||
                         targetKnowledge.TargetDistanceXZ < castRangeMax)
                {
                    actionState.Status = SkillStatus.Prepared;
                }

            }

            //Prepared Status: meet all the requirements, cast skill
            if (actionState.Status == SkillStatus.Prepared)
            {
                if (decision.act == ActDecision.CombatSkill)
                {
                    var skillInfo = actionState.Skill;

                    var targetPosition = aiKnowledge.TargetKnowledge.TargetPosition;
                    if (aiKnowledge.TargetKnowledge.TargetType == AITargetType.EntityTarget)
                    {
                        targetPosition = aiKnowledge.TargetKnowledge.TargetEntity.Position;
                    }

                    var currentPosition = aiKnowledge.CurrentPos;

                    targetPosition.y = 0;
                    currentPosition.y = 0;
                    //face target all the time when playing the skill
                    if (skillInfo.Config.FaceTarget)
                    {
                        // aiKnowledge.desiredForward = (targetPosition - currentPosition).normalized;
                    }

                    CastSkill();
                    actionState.Status = SkillStatus.Playing;
                }
            }

            //Playing Status: play the skill's animation
            if (actionState.Status == SkillStatus.Playing)
            {
                var skillInfo = actionState.Skill;

                bool isFinished = true;

                if (skillInfo.Config.StateIds?.Length > 0)
                {
                    if (actionState.CurrentStateID == null)
                        return;

                    for (int i = 0; i < skillInfo.Config.StateIds.Length; i++)
                    {
                        if (actionState.CurrentStateID == skillInfo.Config.StateIds[i])
                        {
                            isFinished = false;
                            break;
                        }
                    }
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

        private void SelectSkill(List<AISkillInfo> skillCandidates, bool skipPrepare = true)
        {
        }

        private void CastSkill()
        {
            var now = GameTimerManager.Instance.GetTimeNow();
            var maic = aiKnowledge.AiOwnerEntity.GetComponent<MonsterAIInputComponent>();
            var skillInfo = actionState.Skill;
            var targetKnowledge = aiKnowledge.TargetKnowledge;
            var skillKnowledge = aiKnowledge.SkillKnowledge;

            maic.TryDoSkill(skillInfo.SkillId);

            if (skillInfo.Config.TriggerCDOnStart)
            {
                skillInfo.TriggerCD(now);
                skillKnowledge.SetSkillGroupCD(skillInfo.Config.SkillGroupCDID, now);
                if (skillInfo.Config.TriggerGCD)
                    aiKnowledge.SkillKnowledge.SetGCD(now);
                if (skillInfo.Config.TriggerGCD)
                    aiKnowledge.AiManager.SetSkillUsed(skillInfo.Config.PublicCDGroup);
            }
        }

        private void OnSkillStart()
        {
            var skillInfo = actionState.Skill;
            var targetKnowledge = aiKnowledge.TargetKnowledge;

            var currentPosition = aiKnowledge.CurrentPos;
            currentPosition.y = 0;

            var targetDistanceXZ = targetKnowledge.TargetDistanceXZ;

            //Set skill anchor position
            var targetPos = currentPosition;

            if (targetDistanceXZ > skillInfo.Config.CastCondition.CastRangeMax)
            {
                var direction = (currentPosition - targetKnowledge.TargetPosition).normalized * skillInfo.Config.CastCondition.CastRangeMax;
                targetPos = targetKnowledge.TargetPosition + direction;
            }
            aiKnowledge.TargetKnowledge.SkillAnchorPosition = targetPos;
        }

        private void OnSkillFinish()
        {
            var now = GameTimerManager.Instance.GetTimeNow();
            var skillInfo = actionState.Skill;
            var skillKnowledge = aiKnowledge.SkillKnowledge;
            if (skillInfo == null)
                return;
            
            if (!skillInfo.Config.TriggerCDOnStart)
            {
                skillInfo.TriggerCD(now);
                skillKnowledge.SetSkillGroupCD(skillInfo.Config.SkillGroupCDID, now);
                if (skillInfo.Config.TriggerGCD)
                    aiKnowledge.SkillKnowledge.SetGCD(now);
                if (skillInfo.Config.TriggerGCD)
                    aiKnowledge.AiManager.SetSkillUsed(skillInfo.Config.PublicCDGroup);
            }


            //Reset action control state
            actionState.Reset();
        }

        private void OnSkillFail(bool needTriggerCD = true)
        {
            var now = GameTimerManager.Instance.GetTimeNow();
            var skillInfo = actionState.Skill;
            var skillKnowledge = aiKnowledge.SkillKnowledge;
            var maic = aiKnowledge.AiOwnerEntity.GetComponent<MonsterAIInputComponent>();
            maic.TryReleaseSkill();
            if (skillInfo != null)
            {
                if (needTriggerCD)
                {
                    skillInfo.TriggerCD(now);
                    skillKnowledge.SetSkillGroupCD(skillInfo.Config.SkillGroupCDID, now);
                    if (skillInfo.Config.TriggerGCD)
                        aiKnowledge.SkillKnowledge.SetGCD(now);
                    if (skillInfo.Config.TriggerGCD)
                        aiKnowledge.AiManager.SetSkillUsed(skillInfo.Config.PublicCDGroup);
                }
            }

            //Reset action control state
            actionState.Reset();
        }
    }
}