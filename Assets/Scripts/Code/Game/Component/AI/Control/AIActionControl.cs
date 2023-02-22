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
            actionState = aiKnowledge.actionControlState;
            component = aiKnowledge.aiOwnerEntity.GetComponent<AIComponent>();
        }

        public void ExecuteAction(AIDecision decision)
        {
            //Inactive Status: no skill stand by, select new skill
            if (actionState.status == SkillStatus.Inactive)
            {
                if (decision.act == ActDecision.OnAlert)
                {
                    if (aiKnowledge.skillKnowledge.SkillsOnAlert.AvailableSkills.Count > 0)
                    {
                        actionState.status = SkillStatus.Preparing;
                        SelectSkill(aiKnowledge.skillKnowledge.SkillsOnAlert.AvailableSkills[0]);
                        OnSkillStart();
                    }
                }

                if (decision.act == ActDecision.CombatSkillPrepare)
                {
                    if (aiKnowledge.skillKnowledge.SkillsCombat.AvailableSkills.Count > 0)
                    {
                        actionState.status = SkillStatus.Preparing;
                        SelectSkill(aiKnowledge.skillKnowledge.SkillsCombat.AvailableSkills[0]);
                        OnSkillStart();
                    }
                }
            }

            //Preparing Status: in order to meet the castRange condition
            if (actionState.status == SkillStatus.Preparing)
            {
                var targetKnowledge = aiKnowledge.targetKnowledge;
                var currentSkill = actionState.skill;

                var castRangeMin = currentSkill.Config.CastCondition.castRangeMin;
                var castRangeMax = currentSkill.Config.CastCondition.castRangeMax;

                var targetPosition = aiKnowledge.targetKnowledge.targetPosition;
                if (aiKnowledge.targetKnowledge.targetType == AITargetType.EntityTarget)
                {
                    targetPosition = aiKnowledge.targetKnowledge.targetEntity.Position;
                }

                var currentPosition = aiKnowledge.currentPos;

                targetPosition.y = 0;
                currentPosition.y = 0;


                if (castRangeMin < 0 && castRangeMax < 0)
                {
                    actionState.status = SkillStatus.Prepared;
                }

                else if (targetKnowledge.skillAnchorDistance < castRangeMax ||
                         targetKnowledge.targetDistanceXZ < castRangeMax)
                {
                    actionState.status = SkillStatus.Prepared;
                }

            }

            //Prepared Status: meet all the requirements, cast skill
            if (actionState.status == SkillStatus.Prepared)
            {
                if (decision.act == ActDecision.CombatSkill)
                {
                    var skillInfo = actionState.skill;

                    var targetPosition = aiKnowledge.targetKnowledge.targetPosition;
                    if (aiKnowledge.targetKnowledge.targetType == AITargetType.EntityTarget)
                    {
                        targetPosition = aiKnowledge.targetKnowledge.targetEntity.Position;
                    }

                    var currentPosition = aiKnowledge.currentPos;

                    targetPosition.y = 0;
                    currentPosition.y = 0;
                    //face target all the time when playing the skill
                    if (skillInfo.Config.FaceTarget)
                    {
                        // aiKnowledge.desiredForward = (targetPosition - currentPosition).normalized;
                    }

                    CastSkill();
                    actionState.status = SkillStatus.Playing;
                }
            }

            //Playing Status: play the skill's animation
            if (actionState.status == SkillStatus.Playing)
            {
                var skillInfo = actionState.skill;

                bool isFinished = true;

                if (skillInfo.Config.StateIds?.Length > 0)
                {
                    if (actionState.currentStateID == null)
                        return;

                    for (int i = 0; i < skillInfo.Config.StateIds.Length; i++)
                    {
                        if (actionState.currentStateID == skillInfo.Config.StateIds[i])
                        {
                            isFinished = false;
                            break;
                        }
                    }
                }

                if (isFinished)
                {
                    OnSkillFinish();
                    actionState.status = SkillStatus.Inactive;
                }
            }
        }

        private void SelectSkill(AISkillInfo skill)
        {
            actionState.skill = skill;
        }

        private void SelectSkill(List<AISkillInfo> skillCandidates, bool skipPrepare = true)
        {
        }

        private void CastSkill()
        {
            var now = GameTimerManager.Instance.GetTimeNow();
            var maic = aiKnowledge.aiOwnerEntity.GetComponent<MonsterAIInputComponent>();
            var skillInfo = actionState.skill;
            var targetKnowledge = aiKnowledge.targetKnowledge;
            var skillKnowledge = aiKnowledge.skillKnowledge;

            maic.TryDoSkill(skillInfo.SkillId);

            if (skillInfo.Config.TriggerCDOnStart)
            {
                skillInfo.TriggerCD(now);
                skillKnowledge.SetSkillGroupCD(skillInfo.Config.SkillGroupCDID, now);
                if (skillInfo.Config.TriggerGCD)
                    aiKnowledge.skillKnowledge.SetGCD(now);
                if (skillInfo.Config.TriggerGCD)
                    aiKnowledge.aiManager.SetSkillUsed(skillInfo.Config.PublicCDGroup);
            }
        }

        private void OnSkillStart()
        {
            var skillInfo = actionState.skill;
            var targetKnowledge = aiKnowledge.targetKnowledge;

            var currentPosition = aiKnowledge.currentPos;
            currentPosition.y = 0;

            var targetDistanceXZ = targetKnowledge.targetDistanceXZ;

            //Set skill anchor position
            var targetPos = currentPosition;

            if (targetDistanceXZ > skillInfo.Config.CastCondition.castRangeMax)
            {
                var direction = (currentPosition - targetKnowledge.targetPosition).normalized * skillInfo.Config.CastCondition.castRangeMax;
                targetPos = targetKnowledge.targetPosition + direction;
            }
            aiKnowledge.targetKnowledge.skillAnchorPosition = targetPos;
        }

        private void OnSkillFinish()
        {
            var now = GameTimerManager.Instance.GetTimeNow();
            var skillInfo = actionState.skill;
            var skillKnowledge = aiKnowledge.skillKnowledge;
            if (skillInfo == null)
                return;
            
            if (!skillInfo.Config.TriggerCDOnStart)
            {
                skillInfo.TriggerCD(now);
                skillKnowledge.SetSkillGroupCD(skillInfo.Config.SkillGroupCDID, now);
                if (skillInfo.Config.TriggerGCD)
                    aiKnowledge.skillKnowledge.SetGCD(now);
                if (skillInfo.Config.TriggerGCD)
                    aiKnowledge.aiManager.SetSkillUsed(skillInfo.Config.PublicCDGroup);
            }


            //Reset action control state
            actionState.Reset();
        }

        private void OnSkillFail(bool needTriggerCD = true)
        {
            var now = GameTimerManager.Instance.GetTimeNow();
            var skillInfo = actionState.skill;
            var skillKnowledge = aiKnowledge.skillKnowledge;
            var maic = aiKnowledge.aiOwnerEntity.GetComponent<MonsterAIInputComponent>();
            maic.TryReleaseSkill();
            if (skillInfo != null)
            {
                if (needTriggerCD)
                {
                    skillInfo.TriggerCD(now);
                    skillKnowledge.SetSkillGroupCD(skillInfo.Config.SkillGroupCDID, now);
                    if (skillInfo.Config.TriggerGCD)
                        aiKnowledge.skillKnowledge.SetGCD(now);
                    if (skillInfo.Config.TriggerGCD)
                        aiKnowledge.aiManager.SetSkillUsed(skillInfo.Config.PublicCDGroup);
                }
            }

            //Reset action control state
            actionState.Reset();
        }
    }
}