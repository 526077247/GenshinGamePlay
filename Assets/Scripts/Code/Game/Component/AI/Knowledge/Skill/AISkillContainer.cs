using System;
using System.Collections.Generic;

namespace TaoTie
{
    public class AISkillContainer : IDisposable
    {
        public ConfigAISkillType SkillType;

        public ListComponent<AISkillInfo> AllSkills;

        public ListComponent<AISkillInfo> AvailableSkills;

        public static AISkillContainer Create(ConfigAISkillType type)
        {
            AISkillContainer res = ObjectPool.Instance.Fetch<AISkillContainer>();
            res.SkillType = type;
            res.AllSkills = ListComponent<AISkillInfo>.Create();
            res.AvailableSkills = ListComponent<AISkillInfo>.Create();
            return res;
        }

        public void AddSkill(ConfigAISkill conf)
        {
            AISkillInfo skill = AISkillInfo.Create(conf);
            AllSkills.Add(skill);
        }
        
        public void Dispose()
        {
            AvailableSkills.Dispose();
            AvailableSkills = null;
            for (int i = 0; i < AllSkills.Count; i++)
            {
                AllSkills[i].Dispose();
            }
            AllSkills.Dispose();
            AllSkills = null;
            SkillType = default;
            ObjectPool.Instance.Recycle(this);
        }
    }
}