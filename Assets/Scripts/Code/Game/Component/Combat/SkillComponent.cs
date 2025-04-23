using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class SkillComponent:Component,IComponent,IComponent<int[]>,IUpdate
    {
        private NumericComponent numericComponent => parent.GetComponent<NumericComponent>();
        private CombatComponent combatComponent => parent.GetComponent<CombatComponent>();
        private MoveComponent moveComponent => parent.GetComponent<MoveComponent>();
        public Dictionary<int, SkillInfo> SkillInfoMap;
        public event Action<int> OnDoSkillEvt;
        #region IComponent

        public void Init()
        {
            Init(null);
        }
        public void Init(int[] defaultSkills)
        {
            SkillInfoMap = new Dictionary<int, SkillInfo>();
            if (defaultSkills != null)
            {
                for (int i = 0; i < defaultSkills.Length; i++)
                {
                    AddSkillInfoByID(defaultSkills[i]);
                }
            }
        }
        public void Destroy()
        {
            foreach (var item in SkillInfoMap)
            {
                item.Value.Dispose();
            }

            SkillInfoMap = null;
        }

        public void Update()
        {
            
        }

        #endregion
        
        public void TryDoSkill(int skillId)
        {
            if (combatComponent == null) return;
            if(!IsSkillInCD(skillId))
            {
                SkillInfoMap[skillId].LastSpellTime = GameTimerManager.Instance.GetTimeNow();
                combatComponent.SelectAttackTarget(true);
                var target = combatComponent.GetAttackTarget();
                if (target is SceneEntity se)
                {
                    moveComponent.CharacterInput.FaceDirection = se.Position - GetParent<SceneEntity>().Position;
                }
                OnDoSkillEvt?.Invoke(skillId);
                combatComponent.UseSkillImmediately(skillId);
            }
        }

        public bool IsSkillInCD(int skillId)
        {
            if (SkillInfoMap.TryGetValue(skillId, out var data))
            {
                var timeNow =  GameTimerManager.Instance.GetTimeNow();
                return timeNow < data.LastSpellTime + data.CD.GetData(numericComponent);
            }
            return true;
        }

        public SkillInfo AddSkillInfoByID(int skillId)
        {
            if (!SkillInfoMap.TryGetValue(skillId, out var res))
            {
                SkillInfoMap[skillId] = res = SkillInfo.Create(skillId);
            }

            return res;
        }
    }
}