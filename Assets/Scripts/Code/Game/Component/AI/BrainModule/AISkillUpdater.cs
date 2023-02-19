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
            bool isInGcd = knowledge.skillKnowledge.NextGCDTick > timeNow;
            for (int i = 0; i < knowledge.skillKnowledge.Skills.Length; i++)
            {
                var container = knowledge.skillKnowledge.Skills[i];
                container.AvailableSkills.Clear();
                for (int j = 0; j < container.AllSkills.Count; j++)
                {
                    var skill = container.AllSkills[i];
                    //target过滤
                    if (!skill.Config.CanUseIfTargetInactive &&
                        knowledge.targetKnowledge.targetType == AITargetType.InvalidTarget) continue;
                    //视线被遮挡
                    if(skill.Config.NeedLineOfSight && !knowledge.targetKnowledge.hasLineOfSight) continue;
                    //自己cd
                    if (skill.NextAvailableUseTick < timeNow) continue;
                    //ai公共id
                    if (!skill.Config.IgnoreGCD && isInGcd) continue;

                    //全局公共组id
                    if (!string.IsNullOrEmpty(skill.Config.PublicCDGroup) &&
                        !knowledge.aiManager.CanUseSkill(skill.Config.PublicCDGroup,
                            knowledge.targetKnowledge.targetEntity)) continue;

                    //ai公共cd组
                    if (knowledge.skillKnowledge.SkillGroupCDs.TryGetValue(skill.Config.SkillGroupCDID,
                            out var cdInfo) && cdInfo.NextCDTick > timeNow)
                        continue;

                    //condition:
                    if (skill.Config.CastCondition != null)
                    {
                        // pose筛选
                        if (skill.Config.CastCondition.PoseIds != null &&
                            !skill.Config.CastCondition.PoseIds.Contains(knowledge.poseID))
                            continue;
                        
                        //角度筛选
                        if (skill.Config.CastCondition.minTargetAngleY>knowledge.targetKnowledge.targetRelativeAngleYawAbs)
                            continue;
                        if (skill.Config.CastCondition.maxTargetAngleY<knowledge.targetKnowledge.targetRelativeAngleYawAbs)
                            continue;
                        if (skill.Config.CastCondition.minTargetAngleXZ>knowledge.targetKnowledge.targetRelativeAnglePitchAbs)
                            continue;
                        if (skill.Config.CastCondition.maxTargetAngleXZ<knowledge.targetKnowledge.targetRelativeAnglePitchAbs)
                            continue;
                        
                        //距离筛选
                        if (skill.Config.CastCondition.pickRangeMin>knowledge.targetKnowledge.targetDistanceXZ)
                            continue;
                        if (skill.Config.CastCondition.pickRangeMax<knowledge.targetKnowledge.targetDistanceXZ)
                            continue;
                        if (skill.Config.CastCondition.pickRangeYMin>knowledge.targetKnowledge.targetDistanceY)
                            continue;
                        if (skill.Config.CastCondition.pickRangeYMax<knowledge.targetKnowledge.targetDistanceY)
                            continue;
                    }
                    container.AvailableSkills.Add(skill);
                }
            }
        }
    }
}