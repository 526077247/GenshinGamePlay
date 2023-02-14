using System;
using System.Collections.Generic;

namespace TaoTie
{
    public class AISkillContainer : IDisposable
    {
        public ConfigAISkillType skillType;

        public List<AISkillInfo> allSkills;

        public List<AISkillInfo> availableSkills;
        
        public void Dispose()
        {
            skillType = default;
        }
    }
}