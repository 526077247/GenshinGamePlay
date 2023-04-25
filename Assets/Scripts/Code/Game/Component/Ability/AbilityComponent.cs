using System.Collections.Generic;

namespace TaoTie
{
    public class AbilityComponent : Component, IComponent<List<ConfigAbility>>
    {
        private bool IsDestroy;
        private LinkedListComponent<ActorAbility> Abilities;

        private LinkedListComponent<ActorModifier> Modifiers;
        private UnOrderMultiMap<string, ActorModifier> ModifierDictionary; //[Ability_Modifier:ActorModifier]

        #region override

        public void Init(List<ConfigAbility> data)
        {
            IsDestroy = false;
            Abilities = LinkedListComponent<ActorAbility>.Create();
            Modifiers = LinkedListComponent<ActorModifier>.Create();
            ModifierDictionary = new UnOrderMultiMap<string, ActorModifier>();
            if (data != null)
            {
                for (int i = 0; i < data.Count; i++)
                {
                    var ability = ActorAbility.Create(Id, data[i], this);
                    Abilities.AddLast(ability);
                    ability.AfterAdd();
                }
            }
        }

        public void Destroy()
        {
            IsDestroy = true;
            foreach (var item in Abilities)
            {
                item.BeforeRemove();
                item.Dispose();
            }

            Abilities.Dispose();

            foreach (var item in Modifiers)
            {
                item.BeforeRemove();
                item.Dispose();
            }

            Modifiers.Dispose();

            ModifierDictionary = null;
        }

        #endregion
        public void AddAbility(ConfigAbility config)
        {
            var ability = ActorAbility.Create(Id, config, this);
            Abilities.AddLast(ability);
            ability.AfterAdd();
        }

        public ActorModifier ApplyModifier(long applierID, ActorAbility ability, string modifierName)
        {
            if (!ability.TryGetConfigAbilityModifier(modifierName, out var config))
            {
                Log.Error(
                    $"ApplyModifier Fail! modifierName = {modifierName} abilityName = {ability.Config.AbilityName}");
                return null;
            }

            string key = ability.Config.AbilityName + "_" + config.ModifierName;
            var list = ModifierDictionary[key];
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
                            Modifiers.AddLast(modifier);
                            ModifierDictionary.Add(key, modifier);
                            modifier.AfterAdd();
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
                Modifiers.AddLast(modifier);
                ModifierDictionary.Add(key, modifier);
                modifier.AfterAdd();
                return modifier;
            }

            return null;
        }

        public void RemoveModifier(ActorModifier modifier)
        {
            if (IsDestroy) return;
            string key = modifier.Ability.Config.AbilityName + "_" + modifier.Config.ModifierName;
            if (ModifierDictionary.Contains(key, modifier))
            {
                modifier.BeforeRemove();
                Modifiers.Remove(modifier);
                ModifierDictionary.Remove(key, modifier);
                modifier.Dispose();
            }
        }

        public void RemoveModifier(string ability, string modifier)
        {
            string key = ability + "_" + modifier;
            for (int i = ModifierDictionary[key].Count - 1; i >= 0; i--)
            {
                ModifierDictionary[key][i].Dispose();
            }
        }

        public void RemoveAbility(ActorAbility ability)
        {
            if (IsDestroy) return;
            if (Abilities.Contains(ability))
            {
                ability.BeforeRemove();
                Abilities.Remove(ability);
                ability.Dispose();
            }
        }

        public void ExecuteAbility(string ability)
        {
            foreach (var item in Abilities)
            {
                if (item.Config.AbilityName == ability)
                {
                    item.Execute();
                }
            }
        }
    }
}