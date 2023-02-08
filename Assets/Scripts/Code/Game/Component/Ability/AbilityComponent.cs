using System.Collections.Generic;

namespace TaoTie
{
    public class AbilityComponent:Component,IComponent<List<ConfigAbility>>
    {
        private bool isDestroy;
        private LinkedListComponent<ActorAbility> abilities;

        private LinkedListComponent<ActorModifier> modifiers;
        private UnOrderMultiMap<string, ActorModifier> modifierDictionary;//[Ability_Modifier:ActorModifier]
        #region override
        public void Init(List<ConfigAbility> data)
        {
            isDestroy = false;
            abilities = LinkedListComponent<ActorAbility>.Create();
            modifiers = LinkedListComponent<ActorModifier>.Create();
            modifierDictionary = new UnOrderMultiMap<string, ActorModifier>();
            for (int i = 0; i < data.Count; i++)
            {
                var ability = ActorAbility.Create(data[i], this);
                abilities.AddLast(ability);
                ability.AfterAdd();
            }
        }
		
        public void Destroy()
        {
            isDestroy = true;
            foreach (var item in abilities)
            {
                item.BeforeRemove();
                item.Dispose();
            }
            abilities.Dispose();
            
            foreach (var item in modifiers)
            {
                item.BeforeRemove();
                item.Dispose();
            }
            modifiers.Dispose();

            modifierDictionary = null;
        }

        #endregion


        public void ApplyModifier(long applierID, ActorAbility ability, string modifierName)
        {
            if (!ability.TryGetConfigAbilityModifier(modifierName, out var config))
            {
                Log.Error($"ApplyModifier Fail! modifierName = {modifierName} abilityName = {ability.Config.AbilityName}");
                return;
            }

            string key = ability.Config.AbilityName + "_" + config.ModifierName;
            var list = modifierDictionary[key];
            if (list.Count > 0)
            {
                // 处理堆叠
                switch (config.StackingType)
                {
                    case StackingType.Unique:
                    {
                        // 只能存在唯一一个
                        return ;
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
                        }
                        return ;
                    }
                    case StackingType.Refresh:
                    {
                        // 刷新已存在的modifier
                        for (int i = 0; i < list.Count; ++i)
                        {
                            list[i].SetDuration(config.Duration);
                        }
                        return ;
                    }
                    case StackingType.Prolong:
                    {
                        // 延长已存在的modifier
                        for (int i = 0; i < list.Count; ++i)
                        {
                            list[i].AddDuration(config.Duration);
                        }
                        return ;
                    }
                }
                return ;
            }
            else
            {
                var modifier = ActorModifier.Create(applierID, config, ability, this);
                modifiers.AddLast(modifier);
                modifierDictionary.Add(key, modifier);
                modifier.AfterAdd();
            }
            
        }
        
        public void RemoveModifier(ActorModifier modifier)
        {
            if(isDestroy) return;
            string key = modifier.Ability.Config.AbilityName + "_" + modifier.Config.ModifierName;
            if (modifierDictionary.Contains(key, modifier))
            {
                modifier.BeforeRemove();
                modifiers.Remove(modifier);
                modifierDictionary.Remove(key, modifier);
                modifier.Dispose();
            }
        }

        public void RemoveModifier(string ability, string modifier)
        {
            string key = ability + "_" + modifier;
            for (int i = modifierDictionary[key].Count-1; i >=0; i--)
            {
                modifierDictionary[key][i].Dispose();
            }
            modifierDictionary.Remove(key);
        }
        
        public void RemoveAbility(ActorAbility ability)
        {
            if(isDestroy) return;
            if (abilities.Contains(ability))
            {
                ability.BeforeRemove();
                abilities.Remove(ability);
                ability.Dispose();
            }
        }

        public void ExecuteAbility(string ability)
        {
            foreach (var item in abilities)
            {
                if (item.Config.AbilityName == ability)
                {
                    item.Execute();
                }
            }
        }
    }
}