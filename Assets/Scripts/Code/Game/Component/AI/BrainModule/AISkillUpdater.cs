using System.Collections.Generic;
using System.Linq;

namespace TaoTie
{
    /// <summary>
    /// 技能模块
    /// </summary>
    public class AISkillUpdater: BrainModuleBase
    {
#if UNITY_EDITOR
        /// <summary>
        /// 已经报过「AI 配了技能、但这个实体身上没有」的 ConfigId。
        ///
        /// 这条缺失以前是完全静默的：<see cref="SkillComponent.IsSkillInCD"/> 对未知技能返回 true，
        /// 技能被当成「永远在 CD」直接过滤掉，表现只是「怪物贴身站着不打人」，日志里什么都没有。
        /// 按技能去重，一个实体最多每个技能喊一次；修好配置后重进场景重新计数。
        /// </summary>
        private HashSet<int> missingSkillReported;
#endif
        protected override void InitInternal()
        {
            base.InitInternal();
#if UNITY_EDITOR
            missingSkillReported = new HashSet<int>();
#endif
        }

        protected override void ClearInternal()
        {
#if UNITY_EDITOR
            missingSkillReported = null;
#endif
            base.ClearInternal();
        }

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
#if UNITY_EDITOR
                    //技能本体不存在：配置对不上（典型是 ConfigActor.Skill.Skills 少了这一条）
                    if (!knowledge.SkillComponent.HasSkill(skill.ConfigId))
                    {
                        if (missingSkillReported != null && missingSkillReported.Add(skill.ConfigId))
                        {
                            Log.Error($"AI 技能 ConfigId={skill.ConfigId} 在实体 {knowledge.Entity?.Id} 的 SkillComponent 里不存在，" +
                                      "该技能永远不会释放。检查 ConfigActor.Skill.Skills 是否少了这一项（怪物与玩家共用 ConfigActor 时尤其容易漏）");
                        }
                        continue;
                    }
#endif
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