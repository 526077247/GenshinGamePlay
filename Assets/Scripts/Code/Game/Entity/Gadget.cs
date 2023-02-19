using System.Collections.Generic;

namespace TaoTie
{
    /// <summary>
    /// 做一些技能生成物、小动物、场景可交互物件什么的小工具
    /// </summary>
    public class Gadget: Unit,IEntity<int>,IEntity<int,GadgetState>
    {
        public override EntityType Type => EntityType.Gadget;

        #region IEntity

        public void Init(int id)
        {
            Init(id, GadgetState.Default);
        }
        public void Init(int id,GadgetState state)
        {
            var gadget = AddComponent<GadgetComponent,int,GadgetState>(id,state);
            ConfigId = gadget.Config.UnitId;
            var entityConfig = ResourcesManager.Instance.LoadConfig<ConfigEntity>(gadget.Config.EntityConfig);
            AddComponent<GameObjectHolderComponent>();
            AddComponent<NumericComponent,ConfigCombatProperty[]>(entityConfig.Combat?.DefaultProperty);
            AddComponent<FsmComponent,ConfigFsmController>(ResourcesManager.Instance.LoadConfig<ConfigFsmController>(Config.FSM));
            AddComponent<CombatComponent>();
            AddComponent<AbilityComponent,List<ConfigAbility>>(ResourcesManager.Instance.LoadConfig<List<ConfigAbility>>(gadget.Config.Abilities));
            if (!string.IsNullOrEmpty(gadget.Config.AIPath))
            {
                AddComponent<AIComponent,ConfigAIBeta>(ResourcesManager.Instance.LoadConfig<ConfigAIBeta>(gadget.Config.AIPath));
            }
        }
        public void Destroy()
        {
            ConfigId = default;
            CampId = 0;
        }

        #endregion
    }
}