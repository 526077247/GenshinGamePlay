using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 怪物
    /// </summary>
    public class Monster: Actor,IEntity<int,Vector3,uint>
    {
        #region IEntity
        
        public override EntityType Type => EntityType.Monster;
        
        public void Init(int configId,Vector3 bornPos,uint campId)
        {
            Position = bornPos;
            CampId = campId;
            var monster = AddComponent<MonsterComponent,int>(configId);
            ConfigId = monster.Config.UnitId;
            ConfigActor = GetActorConfig(Config.ActorConfig);
            if(ConfigActor.Common!=null) LocalScale = Vector3.one * ConfigActor.Common.Scale;
            AddComponent<UnitModelComponent,ConfigModel>(ConfigActor.Model);
            AddComponent<NumericComponent,ConfigCombatProperty[]>(ConfigActor.Combat?.DefaultProperty);
            AddComponent<FsmComponent,ConfigFsmController>(GetFsmConfig(Config.FSM));
            AddComponent<CombatComponent,ConfigCombat>(ConfigActor.Combat);
            using ListComponent<ConfigAbility> list = ConfigAbilityCategory.Instance.GetList(ConfigActor.Abilities);
            AddComponent<AbilityComponent,List<ConfigAbility>>(list);
            if (!string.IsNullOrEmpty(monster.Config.AIPath))
            {
                var config = GetAIConfig(monster.Config.AIPath);
                if(config!=null && config.Enable)
                    AddComponent<AIComponent,ConfigAIBeta>(config);
            }
            if (!string.IsNullOrEmpty(monster.Config.PoseFSM))
            {
                AddComponent<PoseFSMComponent,ConfigFsmController>(GetFsmConfig(monster.Config.PoseFSM));
            }
            AddComponent<BillboardComponent, ConfigBillboard>(ConfigActor.Billboard);
            AddComponent<MoveComponent>();
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