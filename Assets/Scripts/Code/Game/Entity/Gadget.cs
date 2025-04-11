using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 做一些技能生成物、小动物、场景可交互物件什么的小工具
    /// </summary>
    public class Gadget: Actor,IEntity,IEntity<int,GadgetState,uint>
    {
        public override EntityType Type => EntityType.Gadget;

        #region IEntity

        public void Init()
        {
            
        }
        public void Init(int id, GadgetState state, uint campId)
        {
            CampId = campId;
            var gadget = AddComponent<GadgetComponent,int,GadgetState>(id,state);
            ConfigId = gadget.Config.UnitId;
            ConfigActor = GetActorConfig(Config.ActorConfig);
            if(ConfigActor.Common!=null) LocalScale = Vector3.one * ConfigActor.Common.Scale;
            if (ConfigActor.Intee != null)
            {
                AddComponent<InteeComponent, ConfigIntee>(ConfigActor.Intee);
            }
            AddComponent<UnitModelComponent,ConfigModel>(ConfigActor.Model);
            AddComponent<NumericComponent,ConfigCombatProperty[]>(ConfigActor.Combat?.DefaultProperty);
            if(!string.IsNullOrEmpty(Config.FSM))
            {
                var fsm = AddComponent<FsmComponent, ConfigFsmController>(GetFsmConfig(Config.FSM));
                fsm.SetData(FSMConst.GadgetState,(int)state);
            }
            AddComponent<CombatComponent,ConfigCombat>(ConfigActor.Combat);
            using ListComponent<ConfigAbility> list = ConfigAbilityCategory.Instance.GetList(ConfigActor.Abilities);
            AddComponent<AbilityComponent,List<ConfigAbility>>(list);
            if (!string.IsNullOrEmpty(gadget.Config.AIPath))
            {
                var config = GetAIConfig(gadget.Config.AIPath);
                if(config!=null && config.Enable)
                    AddComponent<AIComponent,ConfigAIBeta,Zone>(config, null);
            }

            AddComponent<BillboardComponent, ConfigBillboard>(ConfigActor.Billboard);
        }
        public void Destroy()
        {
            ConfigActor = null;
            ConfigId = default;
            CampId = 0;
        }


        #endregion
    }
}