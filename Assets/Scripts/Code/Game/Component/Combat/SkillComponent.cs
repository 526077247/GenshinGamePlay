using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class SkillComponent:Component,IComponent,IComponent<ConfigSkillInfo[]>
    {
        private NumericComponent numericComponent => parent.GetComponent<NumericComponent>();
        private CombatComponent combatComponent => parent.GetComponent<CombatComponent>();
        private MoveComponent moveComponent => parent.GetComponent<MoveComponent>();
        public Dictionary<int, SkillInfo> SkillInfoMap;
        public Dictionary<int, SkillInfo> SkillInfoMapLocalId;
        public event Action<int> OnDoSkillEvt;
        #region IComponent

        public void Init()
        {
            Init(null);
        }
        public void Init(ConfigSkillInfo[] skills)
        {
            SkillInfoMap = new Dictionary<int, SkillInfo>();
            SkillInfoMapLocalId = new Dictionary<int, SkillInfo>();
            if (skills != null)
            {
                for (int i = 0; i < skills.Length; i++)
                {
                    AddSkillInfo(skills[i]);
                }
            }
            Messager.Instance.AddListener<string>(Id,MessageId.ExecuteAbility,OnExecuteAbilityEvt);
        }
        public void Destroy()
        {
            Messager.Instance.RemoveListener<string>(Id,MessageId.ExecuteAbility,OnExecuteAbilityEvt);
            foreach (var item in SkillInfoMap)
            {
                item.Value.Dispose();
            }

            SkillInfoMap = null;
        }
        

        private void OnExecuteAbilityEvt(string name)
        {
            foreach (var item in SkillInfoMap)
            {
                if (item.Value.SkillConfig.TriggerCDType == (int)TriggerCDType.OnExecuteAbility
                    && name == item.Value.SkillConfig.AbilityName)
                {
                    TriggerSkillCD(item.Value.ConfigId);
                }
            }
        }

        #endregion
        
        public void TryDoSkill(int configId)
        {
            if (combatComponent == null) return;
            if(!IsSkillInCD(configId) && SkillInfoMap.TryGetValue(configId, out var info))
            {
                if (info.SkillConfig.TriggerCDType == (int) TriggerCDType.OnSpell)
                    TriggerSkillCD(configId);
                combatComponent.SelectAttackTarget(true);
                var target = combatComponent.GetAttackTarget();
                if (target is SceneEntity se)
                {
                    moveComponent.CharacterInput.FaceDirection = se.Position - GetParent<SceneEntity>().Position;
                }
                OnDoSkillEvt?.Invoke(configId);
                combatComponent.UseSkillImmediately(info.SkillId);
            }
        }
        public void TryDoSkillById(int localId)
        {
            if (combatComponent == null) return;
            if (SkillInfoMapLocalId.TryGetValue(localId, out var info))
            {
                var configId = info.ConfigId;
                if(!IsSkillInCD(configId))
                {
                    if (info.SkillConfig.TriggerCDType == (int) TriggerCDType.OnSpell)
                        TriggerSkillCD(configId);
                    combatComponent.SelectAttackTarget(true);
                    var target = combatComponent.GetAttackTarget();
                    if (target is SceneEntity se)
                    {
                        moveComponent.CharacterInput.FaceDirection = se.Position - GetParent<SceneEntity>().Position;
                    }
                    OnDoSkillEvt?.Invoke(configId);
                    combatComponent.UseSkillImmediately(info.SkillId);
                }
            }
        }
        public bool IsSkillInCD(int configId)
        {
            if (SkillInfoMap.TryGetValue(configId, out var data))
            {
                var timeNow =  GameTimerManager.Instance.GetTimeNow();
                return timeNow < data.LastSpellTime + data.CD.GetData(numericComponent);
            }
            return true;
        }

        public SkillInfo AddSkillInfo(ConfigSkillInfo skill)
        {
            if (!SkillInfoMap.TryGetValue(skill.ConfigId, out var res))
            {
                SkillInfoMapLocalId[skill.LocalId] = SkillInfoMap[skill.ConfigId] = res = SkillInfo.Create(skill);
            }

            return res;
        }

        public void TriggerSkillCD(int configId)
        {
            if (SkillInfoMap.TryGetValue(configId, out var info))
            {
                info.LastSpellTime = GameTimerManager.Instance.GetTimeNow();
            }
        }
    }
}