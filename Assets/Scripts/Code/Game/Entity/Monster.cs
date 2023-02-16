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
            ConfigId = configId;
            AddComponent<GameObjectHolderComponent>();
            AddComponent<NumericComponent>();
            
            AddComponent<FsmComponent,ConfigFsmController>(ResourcesManager.Instance.LoadConfig<ConfigFsmController>(Config.FSM));
            AddComponent<CombatComponent>();
            AddComponent<MonsterAIInputComponent>();
            AddComponent<AbilityComponent,List<ConfigAbility>>(ResourcesManager.Instance.LoadConfig<List<ConfigAbility>>(Config.Abilities));
            AddComponent<AIComponent>();
        }
        
        public void Destroy()
        {
            ConfigId = default;
        }
        
        #endregion
    }
}