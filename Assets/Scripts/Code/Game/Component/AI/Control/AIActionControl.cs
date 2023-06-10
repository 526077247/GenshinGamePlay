using System.Collections.Generic;
using UnityEngine;

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
                        SelectSkill(knowledge.SkillKnowledge.SkillsOnAware.AvailableSkills);
                        CastSkill();
                        actionState.Status = SkillStatus.Querying;
                    }
                }
                else if (decision.Act == ActDecision.OnAlert)
                {
                    if (knowledge.SkillKnowledge.SkillsOnAlert.AvailableSkills.Count > 0)
                    {
                        SelectSkill(knowledge.SkillKnowledge.SkillsOnAlert.AvailableSkills);
                        CastSkill();
                        actionState.Status = SkillStatus.Querying;
                    }
                }
                else if (decision.Act == ActDecision.FreeSkill)
                {
                    if (knowledge.SkillKnowledge.SkillsFree.AvailableSkills.Count > 0)
                    {
                        SelectSkill(knowledge.SkillKnowledge.SkillsFree.AvailableSkills);
                        CastSkill();
                        actionState.Status = SkillStatus.Querying;
                    }
                }
                else if (decision.Act == ActDecision.BuddySkill)
                {
                    if (knowledge.SkillKnowledge.SkillsCombatBuddy.AvailableSkills.Count > 0)
                    {
                        SelectSkill(knowledge.SkillKnowledge.SkillsCombatBuddy.AvailableSkills);
                        CastSkill();
                        actionState.Status = SkillStatus.Querying;
                    }
                }
                else if (decision.Act == ActDecision.CombatSkill)
                {
                    if (knowledge.SkillKnowledge.SkillsCombat.AvailableSkills.Count > 0)
                    {
                        SelectSkill(knowledge.SkillKnowledge.SkillsCombat.AvailableSkills);
                        CastSkill();
                        actionState.Status = SkillStatus.Querying;
                    }
                }
                else if (decision.Act == ActDecision.CombatSkillPrepare)
                {
                    if (knowledge.SkillKnowledge.SkillsCombat.AvailableSkills.Count > 0)
                    {
                        actionState.Status = SkillStatus.Preparing;
                        SelectSkill(knowledge.SkillKnowledge.SkillsCombat.AvailableSkills);
                        OnSkillStart();
                    }
                }
            }

            if (actionState.Status == SkillStatus.Preparing)
            {
                var currentSkill = actionState.Skill;
                if (!currentSkill.Config.EnableSkillPrepare)
                {
                    actionState.Status = SkillStatus.Prepared;
                }
                else
                {
                    var targetKnowledge = knowledge.TargetKnowledge;
                    float rangeMin = -1;
                    float rangeMax = -1;
                    if (currentSkill.Config.CastCondition != null)
                    {
                        rangeMin = currentSkill.Config.CastCondition.CastRangeMin;
                        rangeMax = currentSkill.Config.CastCondition.CastRangeMax;
                    }
                
                    if (rangeMin < 0 && rangeMax < 0)
                    {
                        actionState.Status = SkillStatus.Prepared;
                    }
                    else if (targetKnowledge.TargetDistanceXZ <= rangeMax &&
                             targetKnowledge.TargetDistanceXZ >= rangeMin)
                    {
                        actionState.Status = SkillStatus.Prepared;
                    }
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
                    actionState.Status = SkillStatus.Querying;
                    
                    if (actionState.Skill.Config.FaceTarget)
                    {
                        knowledge.Mover.ForceLookAt(targetPosition);
                    }
                }
            }
            
            if (actionState.Status == SkillStatus.Querying)
            {
                if (actionState.QuerySkillDiscardTick < GameTimerManager.Instance.GetTimeNow())
                {
                    OnSkillFail();
                    return;
                }
                bool isplay = false;
                var skillInfo = actionState.Skill;
                if (skillInfo.Config.StateIds?.Length > 0)
                {
                    for (int i = 0; i < skillInfo.Config.StateIds.Length; i++)
                    {
                        if (fsm.DefaultFsm.CurrentStateName == skillInfo.Config.StateIds[i])
                        {
                            isplay = true;
                            break;
                        }
                    }
                }

                if (isplay)
                {
                    actionState.Status = SkillStatus.Playing;
                }
                else
                {
                    knowledge.CombatComponent.UseSkillImmediately(actionState.Skill.SkillId);
                }
            }

            
            if (actionState.Status == SkillStatus.Playing)
            {
                var skillInfo = actionState.Skill;
                
                var rangeMin = skillInfo.Config.CastCondition.SkillAnchorRangeMin;
                var rangeMax = skillInfo.Config.CastCondition.SkillAnchorRangeMax;
                if (rangeMin > 0 && rangeMax > 0) 
                {
                    if (knowledge.TargetKnowledge.SkillAnchorDistance <= rangeMax &&
                        knowledge.TargetKnowledge.SkillAnchorDistance >= rangeMin)//技能释放中被击退到可释放范围外
                    {
                        OnSkillFail();
                        return;
                    }
                }

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

        private void SelectSkill(List<AISkillInfo> skillCandidates, bool skipPrepare = false)
        {
            if (skillCandidates.Count == 1)
            {
                SelectSkill(skillCandidates[0]);
                if (skipPrepare)
                {
                    actionState.Status = SkillStatus.Prepared;
                }
                return;
            }
            int total = 0;
            for (int i = 0; i < skillCandidates.Count; i++)
            {
                total += skillCandidates[i].Config.Weights;
            }

            var value = Random.Range(0, total * 10) % 10;
            for (int i = 0; i < skillCandidates.Count; i++)
            {
                value -= skillCandidates[i].Config.Weights;
                if (value <= 0)
                {
                    actionState.Skill = skillCandidates[i];
                    if (skipPrepare)
                    {
                        actionState.Status = SkillStatus.Prepared;
                    }
                    
                    return;
                }
            }
            OnSkillFail();
        }
        private void CastSkill()
        {
            var now = GameTimerManager.Instance.GetTimeNow();
            var maic = knowledge.CombatComponent;
            var skillInfo = actionState.Skill;
            var skillKnowledge = knowledge.SkillKnowledge;
            
            actionState.QuerySkillDiscardTick = now + skillInfo.Config.SkillQueryingTime;
            
            maic.UseSkillImmediately(skillInfo.SkillId);

            if (skillInfo.Config.TriggerCDOnStart)
            {
                skillInfo.TriggerCD(now);
                skillKnowledge.SetSkillGroupCD(skillInfo.Config.SkillGroupCDID, now);
                if (skillInfo.Config.TriggerGCD)
                    knowledge.SkillKnowledge.SetGCD(now);
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