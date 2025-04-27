using System.Linq;

namespace TaoTie
{
    /// <summary>
    /// 技能模块
    /// </summary>
    public class AISkillUpdater: BrainModuleBase
    {
        protected override void UpdateMainThreadInternal()
        {
            base.UpdateMainThreadInternal();
            var timeNow = GameTimerManager.Instance.GetTimeNow();
            bool isInGcd = knowledge.SkillKnowledge.NextGCDTick > timeNow;
            for (int i = 0; i < knowledge.SkillKnowledge.Skills.Length; i++)
            {
                var container = knowledge.SkillKnowledge.Skills[i];
                container.AvailableSkills.Clear();
                for (int j = 0; j < container.AllSkills.Count; j++)
                {
                    var skill = container.AllSkills[j];
                    //target过滤
                    if (!skill.Config.CanUseIfTargetInactive &&
                        knowledge.TargetKnowledge.TargetType == AITargetType.InvalidTarget) continue;
                    //视线被遮挡
                    if(skill.Config.NeedLineOfSight && !knowledge.TargetKnowledge.HasLineOfSight) continue;
                    //自己cd
                    if(knowledge.SkillComponent.IsSkillInCD(skill.ConfigId)) continue;
                    //自己ai技能cd
                    if (skill.NextAvailableUseTick > timeNow) continue;
                    //ai公共cd
                    if (!skill.Config.IgnoreGCD && isInGcd) continue;

                    //全局公共组cd
                    if (!string.IsNullOrEmpty(skill.Config.PublicCDGroup) &&
                        !knowledge.AIManager.CanUseSkill(skill.Config.PublicCDGroup,
                            knowledge.TargetKnowledge.TargetEntity)) continue;

                    //ai公共cd组
                    if (knowledge.SkillKnowledge.SkillGroupCDs.TryGetValue(skill.Config.SkillGroupCDID,
                            out var cdInfo) && cdInfo.NextCDTick > timeNow)
                        continue;

                    //condition:
                    if (skill.Config.CastCondition != null)
                    {
                        // pose筛选
                        if (skill.Config.CastCondition.PoseIds != null &&
                            !skill.Config.CastCondition.PoseIds.Contains(knowledge.PoseID))
                            continue;
                        
                        //角度筛选
                        if (skill.Config.CastCondition.MinTargetAngleY>knowledge.TargetKnowledge.TargetRelativeAngleYawAbs)
                            continue;
                        if (skill.Config.CastCondition.MaxTargetAngleY<knowledge.TargetKnowledge.TargetRelativeAngleYawAbs)
                            continue;
                        if (skill.Config.CastCondition.MinTargetAngleXZ>knowledge.TargetKnowledge.TargetRelativeAnglePitchAbs)
                            continue;
                        if (skill.Config.CastCondition.MaxTargetAngleXZ<knowledge.TargetKnowledge.TargetRelativeAnglePitchAbs)
                            continue;
                        
                        //距离筛选
                        if (skill.Config.CastCondition.PickRangeMin>knowledge.TargetKnowledge.TargetDistanceXZ)
                            continue;
                        if (skill.Config.CastCondition.PickRangeMax<knowledge.TargetKnowledge.TargetDistanceXZ)
                            continue;
                        if (skill.Config.CastCondition.PickRangeYMin>knowledge.TargetKnowledge.TargetDistanceY)
                            continue;
                        if (skill.Config.CastCondition.PickRangeYMax<knowledge.TargetKnowledge.TargetDistanceY)
                            continue;
                    }
                    container.AvailableSkills.Add(skill);
                }
            }
        }
    }
}