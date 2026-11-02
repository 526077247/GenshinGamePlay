using System;
using System.Collections.Generic;

namespace TaoTie
{
    public class AbilityComponent : Component, IComponent<List<ConfigAbility>>
    {
        private bool isDestroy;
        private LinkedListComponent<ActorAbility> abilities;

        private LinkedListComponent<ActorModifier> modifiers;
        private UnOrderMultiMap<string, ActorModifier> modifierDictionary; //[Ability_Modifier:ActorModifier]

        /// <summary>
        /// 活跃 modifier 链表（只读遍历）。供 <see cref="ControlComponent"/> 全量聚合控制位，
        /// 用链表直读而不是回调委托，避免每次重算都分配闭包。
        /// 不要在外部增删，统一走 ApplyModifier / RemoveModifier。
        /// </summary>
        public LinkedListComponent<ActorModifier> Modifiers => modifiers;
        
        #region override

        public void Init(List<ConfigAbility> data)
        {
            isDestroy = false;
            abilities = LinkedListComponent<ActorAbility>.Create();
            modifiers = LinkedListComponent<ActorModifier>.Create();
            modifierDictionary = new UnOrderMultiMap<string, ActorModifier>();
            if (data != null)
            {
                for (int i = 0; i < data.Count; i++)
                {
                    var ability = ActorAbility.Create(Id, data[i], this);
                    abilities.AddLast(ability);
                    ability.AfterAdd();
                }
            }
        }

        public void Destroy()
        {
            if (isDestroy) return;
            isDestroy = true;
            
            for (var node = abilities.Last; node != null;)
            {
                var prev = node.Previous;
                node.Value.BeforeRemove();
                node.Value.Dispose();
                node = prev;
            }

            abilities.Dispose();

            for (var node = modifiers.Last; node != null;)
            {
                var prev = node.Previous;
                node.Value.BeforeRemove();
                node.Value.Dispose();
                node = prev;
            }

            modifiers.Dispose();

            modifierDictionary = null;
        }

        #endregion
        public ActorAbility AddAbility(ConfigAbility config)
        {
            //组件销毁中禁止再添加
            if (isDestroy) return null;
            var ability = ActorAbility.Create(Id, config, this);
            abilities.AddLast(ability);
            ability.AfterAdd();
            return ability;
        }
        public void AddAbilities(List<ConfigAbility> configs)
        {
            for (int i = 0; i < configs.Count; i++)
            {
                AddAbility(configs[i]);
            }
        }

        public ActorModifier ApplyModifier(long applierID, ActorAbility ability, string modifierName)
        {
            //组件销毁中禁止再添加
            if (isDestroy) return null;
            if (!ability.TryGetConfigAbilityModifier(modifierName, out var config))
            {
                Log.Error(
                    $"ApplyModifier Fail! modifierName = {modifierName} abilityName = {ability.Config.AbilityName}");
                return null;
            }

            string key = ability.Config.AbilityName + "_" + config.ModifierName;
            var list = modifierDictionary[key];
            if (list!=null && list.Count > 0)
            {
                // 处理堆叠
                switch (config.StackingType)
                {
                    case StackingType.Unique:
                    {
                        // 只能存在唯一一个
                        return null;
                    }
                    case StackingType.Multiple:
                    {
                        // 互相独立存在,且有层数数量限制
                        int limitNum = int.MaxValue;
                        if (config.StackLimitCount > 0)
                        {
                            limitNum = config.StackLimitCount;
                        }

                        if (list.Count < limitNum)
                        {
                            var modifier = ActorModifier.Create(applierID, config, ability, this);
                            modifiers.AddLast(modifier);
                            modifierDictionary.Add(key, modifier);
                            modifier.AfterAdd();
                            NotifyControlChanged();
                            return modifier;
                        }

                        return  null;
                    }
                    case StackingType.Refresh:
                    {
                        // 刷新已存在的modifier
                        for (int i = 0; i < list.Count; ++i)
                        {
                            list[i].SetDuration(config.Duration);
                        }

                        return  null;
                    }
                    case StackingType.Prolong:
                    {
                        // 延长已存在的modifier
                        for (int i = 0; i < list.Count; ++i)
                        {
                            list[i].AddDuration(config.Duration);
                        }

                        return null;
                    }
                }
            }
            else
            {
                var modifier = ActorModifier.Create(applierID, config, ability, this);
                modifiers.AddLast(modifier);
                modifierDictionary.Add(key, modifier);
                modifier.AfterAdd();
                NotifyControlChanged();
                return modifier;
            }

            return null;
        }

        public void RemoveModifier(ActorModifier modifier)
        {
            if (isDestroy || modifier == null) return;
            string key = modifier.Ability?.Config?.AbilityName + "_" + modifier.Config?.ModifierName;
            if (modifierDictionary.Contains(key, modifier))
            {
                modifier.BeforeRemove();
                modifiers.Remove(modifier);
                modifierDictionary.Remove(key, modifier);
                modifier.Dispose();
                NotifyControlChanged();
            }
        }

        public void RemoveModifier(string ability, string modifier)
        {
            if (isDestroy) return;
            string key = ability + "_" + modifier;
            var list = modifierDictionary[key];
            if (list == null || list.Count == 0) return;
            if (list.Count == 1)
            {
                RemoveModifier(list[0]);
                return;
            }

            var snapshot = list.ToArray();
            for (int i = 0; i < snapshot.Length; i++)
            {
                RemoveModifier(snapshot[i]);
            }
        }

        public void RemoveAbility(ActorAbility ability)
        {
            if (isDestroy) return;
            if (abilities.Contains(ability))
            {
                ability.BeforeRemove();
                abilities.Remove(ability);
                ability.Dispose();
            }
        }
        public void RemoveAbility(string abilityName)
        {
            if (isDestroy) return;
            ActorAbility ability = null;
            foreach (var item in abilities)
            {
                if (item.Config.AbilityName == abilityName)
                {
                    ability = item;
                    break;
                }
            }
            if(ability==null) return;
            if (abilities.Contains(ability))
            {
                ability.BeforeRemove();
                abilities.Remove(ability);
                ability.Dispose();
            }
        }
        /// <summary>
        /// 按名字查已有实例（只读）。心法注入前必须先查：
        /// <see cref="AddAbility"/> 不去重，同名 Add 两次会得到两个 ActorAbility，效果翻倍。
        /// </summary>
        public ActorAbility GetAbility(string abilityName)
        {
            if (abilities == null) return null;
            for (var node = abilities.First; node != null; node = node.Next)
            {
                var a = node.Value;
                if (a?.Config != null && a.Config.AbilityName == abilityName) return a;
            }
            return null;
        }

        /// <summary>
        /// modifier 增删后通知控制仲裁者重算能力位掩码。
        /// 组件销毁过程中不通知：此时 isDestroy 已置位，销毁链路里的 RemoveModifier 也会提前返回，
        /// 没必要再去碰一个可能已经 Dispose 的 ControlComponent。
        /// </summary>
        private void NotifyControlChanged()
        {
            if (isDestroy) return;
            GetComponent<ControlComponent>()?.NotifyModifiersChanged();
        }

        public void ExecuteAbility(string ability)
        {
            foreach (var item in abilities)
            {
                if (item.Config.AbilityName == ability)
                {
                    Messager.Instance.Broadcast(Id,MessageId.ExecuteAbility,ability);
                    item.Execute();
                }
            }
        }
    }
}