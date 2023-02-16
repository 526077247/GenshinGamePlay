using System.Collections.Generic;

namespace TaoTie
{
    /// <summary>
    /// 玩家
    /// </summary>
    public class Avatar:Unit,IEntity<int>
    {
        #region IEntity

        public override EntityType Type => EntityType.Avatar;

        public void Init(int configId)
        {
            ConfigId = configId;
            AddComponent<GameObjectHolderComponent>();
            AddComponent<NumericComponent>();
            
            AddComponent<FsmComponent,ConfigFsmController>(ResourcesManager.Instance.LoadConfig<ConfigFsmController>(Config.FSM));
            AddComponent<CombatComponent>();
            AddComponent<SkillComponent>();
            AddComponent<LocalInputController>();
            AddComponent<AbilityComponent,List<ConfigAbility>>(ResourcesManager.Instance.LoadConfig<List<ConfigAbility>>(Config.Abilities));

        }

        public void Destroy()
        {
            ConfigId = default;
        }

        #endregion
        
    }
}