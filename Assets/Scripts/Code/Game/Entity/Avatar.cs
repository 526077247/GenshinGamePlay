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
            CampId = CampConst.Player;
            var avatar = AddComponent<AvatarComponent,int>(configId);
            ConfigId = avatar.Config.UnitId;
            var entityConfig = ResourcesManager.Instance.LoadConfig<ConfigEntity>(avatar.Config.EntityConfig);
            AddComponent<GameObjectHolderComponent>();
            AddComponent<NumericComponent,ConfigCombatProperty[]>(entityConfig.Combat?.DefaultProperty);
            AddComponent<FsmComponent,ConfigFsmController>(ResourcesManager.Instance.LoadConfig<ConfigFsmController>(Config.FSM));
            AddComponent<CombatComponent>();
            AddComponent<SkillComponent>();
            AddComponent<LocalInputController>();
            AddComponent<AbilityComponent,List<ConfigAbility>>(ResourcesManager.Instance.LoadConfig<List<ConfigAbility>>(avatar.Config.Abilities));

        }

        public void Destroy()
        {
            ConfigId = default;
        }

        #endregion
        
    }
}