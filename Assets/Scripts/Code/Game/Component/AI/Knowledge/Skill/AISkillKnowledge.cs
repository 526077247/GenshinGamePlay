using System;
using System.Collections.Generic;

namespace TaoTie
{
    public class AISkillKnowledge: IDisposable
    {
        public AISkillContainer[] Skills;

        public AISkillContainer SkillsOnAware => Skills[(int)ConfigAISkillType.OnAware];
        public AISkillContainer SkillsOnAlert => Skills[(int)ConfigAISkillType.OnAlert];
        public AISkillContainer SkillsOnNerve => Skills[(int)ConfigAISkillType.OnNerve];
        public AISkillContainer SkillsCombat => Skills[(int)ConfigAISkillType.Combat];
        public AISkillContainer SkillsFree => Skills[(int)ConfigAISkillType.Free];
        public AISkillContainer SkillsActionPoint => Skills[(int)ConfigAISkillType.ActionPoint];
        public AISkillContainer SkillsCombatBuddy => Skills[(int)ConfigAISkillType.CombatBuddy];
        
        public DictionaryComponent<int, AISkillGroupCDInfo> SkillGroupCDs;
        
        // public Dictionary<int, List<AISkillInfo>> NeedResetCDSkills;
        
        // public HashSet<string> CaredTargetGlobalValueNames;
        // public Dictionary<string, float> CaredTargetGlobalValues;
        
        public int Gcd;
        public long NextGCDTick;
        
        public uint SkillCount;

        // public float CdMultiplier;
        public long LastCastSkillTick;
        // public byte CurrentSkillEliteSetID;

        public static AISkillKnowledge Create(ConfigAIBeta config)
        {
            var res = ObjectPool.Instance.Fetch<AISkillKnowledge>();
            
            res.Skills = new AISkillContainer[(int)ConfigAISkillType.Max];
            for (int i = 0; i < res.Skills.Length; i++)
            {
                res.Skills[i] = AISkillContainer.Create((ConfigAISkillType)i);
            }

            if (config.Skills != null)
            {
                for (int i = 0; i < config.Skills.Length; i++)
                {
                    var conf = config.Skills[i];
                    res.Skills[(int) conf.SkillType].AddSkill(conf);
                }
                res.SkillCount = (uint)config.Skills.Length;
            }
            
            res.Gcd = config.GloabCD;
            res.NextGCDTick = 0;
            res.SkillGroupCDs = DictionaryComponent<int, AISkillGroupCDInfo>.Create();
            if (config.SkillGroupCDConfigs != null)
            {
                foreach (var item in config.SkillGroupCDConfigs)
                {
                    res.SkillGroupCDs.Add(item.Key, AISkillGroupCDInfo.Create(item.Key, item.Value));
                }
            }
            return res;
        }

        public void Dispose()
        {
            Gcd = 0;
            NextGCDTick = 0;
            SkillCount = 0;
            LastCastSkillTick = 0;
            for (int i = 0; i < Skills.Length; i++)
            {
                Skills[i].Dispose();
            }

            Skills = null;
            foreach (var item in SkillGroupCDs)
            {
                item.Value.Dispose();
            }
            SkillGroupCDs.Dispose();
            SkillGroupCDs = null;
            ObjectPool.Instance.Recycle(this);
        }
    }
}