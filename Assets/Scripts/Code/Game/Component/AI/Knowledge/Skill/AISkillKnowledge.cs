using System;
using System.Collections.Generic;

namespace TaoTie
{
    public class AISkillKnowledge: IDisposable
    {
        public List<AISkillContainer> skills;
        public Dictionary<int, List<AISkillInfo>> needResetCDSkills;
        public AISkillContainer skillsOnAware;
        public AISkillContainer skillsOnAlert;
        public AISkillContainer skillsOnNerve;
        public AISkillContainer skillsCombat;
        public AISkillContainer skillsFree;
        public AISkillContainer skillsActionPoint;
        public AISkillContainer skillsCombatBuddy;
        public Dictionary<int, AISkillGroupCDInfo> skillGroupCDs;
        public HashSet<string> caredTargetGlobalValueNames;
        public Dictionary<string, float> caredTargetGlobalValues;
        public int gcd;
        public uint skillCount;
        public long nextGCDTick;
        public float cdMultiplier;
        public long lastCastSkillTick;
        public byte currentSkillEliteSetID;

        public void Dispose()
        {
            for (int i = 0; i < skills.Count; i++)
            {
                skills[i].Dispose();
            }

            skills = null;
        }
    }
}