using System.Collections.Generic;

namespace TaoTie
{
    /// <summary>
    /// 做一些技能生成物、小动物、场景可交互物件什么的小工具
    /// </summary>
    public class Gadget: Unit,IEntity<int,uint>,IEntity<int,GadgetState,uint>
    {
        public override EntityType Type => EntityType.Gadget;

        #region IEntity

        public void Init(int id,uint campId)
        {
            Init(id, GadgetState.Default,campId);
        }
        public void Init(int id,GadgetState state,uint campId)
        {
            CampId = campId;
            var gadget = AddComponent<GadgetComponent,int,GadgetState>(id,state);
            ConfigId = gadget.Config.UnitId;
            ConfigEntity = ResourcesManager.Instance.LoadConfig<ConfigEntity>(Config.EntityConfig);
            AddComponent<GameObjectHolderComponent>();
            AddComponent<NumericComponent,ConfigCombatProperty[]>(ConfigEntity.Combat?.DefaultProperty);
            AddComponent<FsmComponent,ConfigFsmController>(ResourcesManager.Instance.LoadConfig<ConfigFsmController>(Config.FSM));
            AddComponent<CombatComponent>();
            using ListComponent<ConfigAbility> list = ConfigAbilityCategory.Instance.GetList(ConfigEntity.Abilities);
            AddComponent<AbilityComponent,List<ConfigAbility>>(list);
            if (!string.IsNullOrEmpty(gadget.Config.AIPath))
            {
                AddComponent<AIComponent,ConfigAIBeta>(ResourcesManager.Instance.LoadConfig<ConfigAIBeta>(gadget.Config.AIPath));
            }
        }
        public void Destroy()
        {
            ConfigEntity = null;
            ConfigId = default;
            CampId = 0;
        }

        #endregion
    }
}