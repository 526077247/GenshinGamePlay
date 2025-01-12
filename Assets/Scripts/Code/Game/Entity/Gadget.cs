using System.Collections.Generic;
using Unity.Code.NinoGen;

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
            configActor = GetActorConfig(Config.ActorConfig);
            if (configActor.Intee != null)
            {
                AddComponent<InteeComponent, ConfigIntee>(configActor.Intee);
            }
            AddComponent<GameObjectHolderComponent>();
            AddComponent<NumericComponent,ConfigCombatProperty[]>(configActor.Combat?.DefaultProperty);
            var fsm = AddComponent<FsmComponent,ConfigFsmController>(GetFsmConfig(Config.FSM));
            fsm.SetData(FSMConst.GadgetState,(int)state);
            AddComponent<CombatComponent,ConfigCombat>(configActor.Combat);
            using ListComponent<ConfigAbility> list = ConfigAbilityCategory.Instance.GetList(configActor.Abilities);
            AddComponent<AbilityComponent,List<ConfigAbility>>(list);
            if (!string.IsNullOrEmpty(gadget.Config.AIPath))
            {
                var config = GetAIConfig(gadget.Config.AIPath);
                if(config!=null && config.Enable)
                    AddComponent<AIComponent,ConfigAIBeta>(config);
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