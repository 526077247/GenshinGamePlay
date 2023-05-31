using System.Collections.Generic;

namespace TaoTie
{
    /// <summary>
    /// 做一些技能生成物、小动物、场景可交互物件什么的小工具
    /// </summary>
    public class Gadget: Actor,IEntity<int,uint>,IEntity<int,GadgetState,uint>
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
            configActor = ResourcesManager.Instance.LoadConfig<ConfigActor>(Config.ActorConfig);
            if (configActor.Intee != null)
            {
                AddComponent<InteeComponent, ConfigIntee>(configActor.Intee);
            }
            AddComponent<GameObjectHolderComponent>();
            AddComponent<NumericComponent,ConfigCombatProperty[]>(configActor.Combat?.DefaultProperty);
            var fsm = AddComponent<FsmComponent,ConfigFsmController>(ResourcesManager.Instance.LoadConfig<ConfigFsmController>(Config.FSM));
            fsm.SetData(FSMConst.GadgetState,(int)state);
            AddComponent<CombatComponent,ConfigCombat>(configActor.Combat);
            using ListComponent<ConfigAbility> list = ConfigAbilityCategory.Instance.GetList(configActor.Abilities);
            AddComponent<AbilityComponent,List<ConfigAbility>>(list);
            if (!string.IsNullOrEmpty(gadget.Config.AIPath))
            {
                AddComponent<AIComponent,ConfigAIBeta>(ResourcesManager.Instance.LoadConfig<ConfigAIBeta>(gadget.Config.AIPath));
            }

            AddComponent<BillboardComponent, ConfigBillboard>(configActor.Billboard);
        }
        public void Destroy()
        {
            configActor = null;
            ConfigId = default;
            CampId = 0;
        }

        #endregion
    }
}