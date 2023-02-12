using System.Collections.Generic;

namespace TaoTie
{
    public class AIComponent: Component,IComponent<ConfigAIBeta>,IUpdateComponent
    {
        public ConfigAIBeta Config { get; private set; }
        public readonly Dictionary<int, SkillInfo> Skill = new Dictionary<int, SkillInfo>();
        /// <summary>
        /// [组:下一次可施法时间]
        /// </summary>
        public readonly Dictionary<string, long> Group = new Dictionary<string, long>();
        #region IComponent

        public void Init(ConfigAIBeta p1)
        {
            Config = p1;
            for (int i = 0; i < p1.Skills.Length; i++)
            {
                var skillInfo = SkillInfo.Create(p1.Skills[i]);
                Skill[skillInfo.SkillId] = skillInfo;
            }
        }

        public void Destroy()
        {
            Config = null;
            foreach (var item in Skill)
            {
                item.Value.Dispose();
            }

            Skill.Clear();
            Group.Clear();
        }

        public void Update()
        {
            UpdateSkill();
            
        }

        #endregion


        #region Skill

        private void UpdateSkill()
        {
            
        }

        #endregion
    }
}