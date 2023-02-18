using System.Collections.Generic;

namespace TaoTie
{
    /// <summary>
    /// 怪物
    /// </summary>
    public class Monster: Unit,IEntity<int>
    {
        #region IEntity
        
        public override EntityType Type => EntityType.Monster;
        
        public void Init(int configId)
        {
            var monster = AddComponent<MonsterComponent,int>(configId);
            ConfigId = monster.Config.UnitId;
            var entityConfig = ResourcesManager.Instance.LoadConfig<ConfigEntity>(monster.Config.EntityConfig);
            AddComponent<GameObjectHolderComponent>();
            AddComponent<NumericComponent,ConfigCombatProperty[]>(entityConfig.Combat?.DefaultProperty);
            AddComponent<FsmComponent,ConfigFsmController>(ResourcesManager.Instance.LoadConfig<ConfigFsmController>(Config.FSM));
            AddComponent<CombatComponent>();
            AddComponent<AbilityComponent,List<ConfigAbility>>(ResourcesManager.Instance.LoadConfig<List<ConfigAbility>>(monster.Config.Abilities));
            if (!string.IsNullOrEmpty(monster.Config.AIPath))
            {
                AddComponent<MonsterAIInputComponent>();
                AddComponent<AIComponent,ConfigAIBeta>(ResourcesManager.Instance.LoadConfig<ConfigAIBeta>(monster.Config.AIPath));
            }
            
        }
        
        public void Destroy()
        {
            ConfigId = default;
        }
        
        #endregion
    }
}