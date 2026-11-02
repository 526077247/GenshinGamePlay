using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class SkillComponent:Component,IComponent,IComponent<ConfigSkill>
    {
        private NumericComponent numericComponent => parent.GetComponent<NumericComponent>();
        private CombatComponent combatComponent => parent.GetComponent<CombatComponent>();
        private MoveComponent moveComponent => parent.GetComponent<MoveComponent>();
        public Dictionary<int, SkillInfo> SkillInfoMap;
        public Dictionary<int, SkillInfo> SkillInfoMapLocalId;
        private int sprintBSId;
        public event Action<int> OnDoSkillEvt;
        #region IComponent

        public void Init()
        {
            Init(null);
        }
        public void Init(ConfigSkill config)
        {
            SkillInfoMap = new Dictionary<int, SkillInfo>();
            SkillInfoMapLocalId = new Dictionary<int, SkillInfo>();
            for (int i = 0; i < config?.Skills?.Length; i++)
            {
                AddSkillInfo(config.Skills[i]);
            }
            if (config?.SprintBS != null)
            {
                sprintBSId = config.SprintBS.ConfigId;
                AddSkillInfo(config.SprintBS);
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
            SkillInfoMapLocalId = null;
            sprintBSId = 0;
        }
        

        private void OnExecuteAbilityEvt(string name)
        {
            foreach (var item in SkillInfoMap)
            {
                if (item.Value.TriggerCDType == (int)TriggerCDType.OnExecuteAbility
                    && name == item.Value.AbilityName)
                {
                    TriggerSkillCD(item.Value.ConfigId);
                }
            }
        }

        #endregion
        
        public void TryDoSkill(int configId)
        {
            if (combatComponent == null) return;
            if (GetComponent<ControlComponent>() is { } ctl && !ctl.CanDo(ControlFlag.CastSkill)) return;
            if(!IsSkillInCD(configId) && SkillInfoMap.TryGetValue(configId, out var info))
            {
                if (info.TriggerCDType == (int) TriggerCDType.OnSpell)
                    TriggerSkillCD(configId);
                combatComponent.SelectAttackTarget(true);
                var target = combatComponent.GetAttackTarget();
                if (target is SceneEntity se && moveComponent != null)
                {
                    moveComponent.ForceLookAt(se.Position);
                }
                OnDoSkillEvt?.Invoke(configId);
                // FsmSkillId<=0 = 瞬发技能：不进 FSM。既不会播技能动画，也不会带出 FSM 时间轴上挂的 Ability
                if (info.SkillId > 0) combatComponent.UseSkillImmediately(info.SkillId);
            }
        }
        public void TryDoSkillById(int localId)
        {
            if (combatComponent == null) return;
            if (GetComponent<ControlComponent>() is { } ctl && !ctl.CanDo(ControlFlag.CastSkill)) return;
            if (SkillInfoMapLocalId.TryGetValue(localId, out var info))
            {
                var configId = info.ConfigId;
                if(!IsSkillInCD(configId))
                {
                    if (info.TriggerCDType == (int) TriggerCDType.OnSpell)
                        TriggerSkillCD(configId);
                    combatComponent.SelectAttackTarget(true);
                    var target = combatComponent.GetAttackTarget();
                    if (target is SceneEntity se && moveComponent != null)
                    {
                        moveComponent.ForceLookAt(se.Position);
                    }
                    OnDoSkillEvt?.Invoke(configId);
                    // 同上：FsmSkillId<=0 的瞬发技能不占用 FSM 状态，避免播一段技能/攻击动画
                    if (info.SkillId > 0) combatComponent.UseSkillImmediately(info.SkillId);
                }
            }
        }
        
        /// <summary>
        /// 批量注入技能（场景模板 / 外部系统用）。
        ///
        /// 走 <see cref="SetSkillInfo"/> 而不是 <see cref="AddSkillInfo"/>：后者遇到同 ConfigId 会
        /// 直接复用旧实例（LocalId 还是旧的），注入就等于什么都没发生；SetSkillInfo 先清同 LocalId /
        /// 同 ConfigId 再注册，重复注入同一份配置既不翻倍、也不会留下指向已 Dispose 实例的映射。
        ///
        /// 后三个参数留空 = 沿用 <see cref="SkillConfig"/> 的默认值（power 只有心法公式会读；
        /// trigger=-1 / ability=null 都是"用配置默认"）。
        /// </summary>
        public void InjectSkills(ConfigSkillInfo[] skills)
        {
            if (skills == null || SkillInfoMap == null) return;
            for (int i = 0; i < skills.Length; i++)
            {
                var info = skills[i];
                if (info == null) continue;
                SetSkillInfo(info, null, -1, null);
            }
        }

        /// <summary>
        /// 技能表里到底有没有这一项。
        ///
        /// <see cref="IsSkillInCD"/> 对<b>未知</b>技能返回 true（视为永远在 CD），这是「查不到就别放」
        /// 的安全口径，但也让「配置里少了一条技能」和「技能正在 CD」表现完全一样。
        /// 需要区分这两者的地方（AI 诊断）先用这个方法问一句。
        /// </summary>
        public bool HasSkill(int configId)
        {
            return SkillInfoMap != null && SkillInfoMap.ContainsKey(configId);
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
        /// <summary>
        /// 冲刺是否CD
        /// </summary>
        /// <returns></returns>
        public bool IsSprintBSInCD()
        {
            if (sprintBSId == 0) return false;
            return IsSkillInCD(sprintBSId);
        }

        public SkillInfo AddSkillInfo(ConfigSkillInfo skill)
        {
            if (!SkillInfoMap.TryGetValue(skill.ConfigId, out var res))
            {
                SkillInfoMapLocalId[skill.LocalId] = SkillInfoMap[skill.ConfigId] = res = SkillInfo.Create(skill);
            }

            return res;
        }

        /// <summary>
        /// 按 LocalId 移除（心法槽位重新装配时用）。
        /// 不能出了事再从外面清字典：ConfigId 已存在时 AddSkillInfo 压根不建 LocalId 映射，
        /// 外部手清一定会漏建/漏删，所以装配只走这三个方法。
        /// </summary>
        public void RemoveSkillInfoByLocalId(int localId)
        {
            if (SkillInfoMapLocalId != null
                && SkillInfoMapLocalId.TryGetValue(localId, out var info)
                && info != null)
            {
                RemoveSkillInfo(info.ConfigId);
            }
        }

        /// <summary>按 ConfigId 移除，两个字典与实例一起清</summary>
        public void RemoveSkillInfo(int configId)
        {
            if (SkillInfoMap == null) return;
            if (!SkillInfoMap.TryGetValue(configId, out var info) || info == null) return;

            SkillInfoMap.Remove(configId);
            // 只移除"确实指向这个实例"的 LocalId 映射，避免误删别人的键
            if (info.Info != null
                && SkillInfoMapLocalId.TryGetValue(info.Info.LocalId, out var mapped)
                && mapped == info)
            {
                SkillInfoMapLocalId.Remove(info.Info.LocalId);
            }

            info.Dispose();
        }

        /// <summary>
        /// 原子装配：先清同 LocalId / 同 ConfigId，再注册，避免两处静默冲突。
        /// 心法注入专用；角色自带技能仍然走 <see cref="AddSkillInfo"/>，行为不变。
        /// </summary>
        public SkillInfo SetSkillInfo(ConfigSkillInfo info, string cdOverride,
            int triggerOverride, string abilityName)
        {
            if (info == null) return null;

            RemoveSkillInfoByLocalId(info.LocalId);
            RemoveSkillInfo(info.ConfigId);

            var res = SkillInfo.Create(info);
            res.SetRuntime(cdOverride, triggerOverride, abilityName);
            SkillInfoMapLocalId[info.LocalId] = SkillInfoMap[info.ConfigId] = res;
            return res;
        }

        public void TriggerSkillCD(int configId)
        {
            if (SkillInfoMap.TryGetValue(configId, out var info))
            {
                info.LastSpellTime = GameTimerManager.Instance.GetTimeNow();
            }
        }
        
        public void TriggerSprintBSCD()
        {
            if (sprintBSId == 0) return;
            TriggerSkillCD(sprintBSId);
        }
    }
}