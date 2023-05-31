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
            configActor = ResourcesManager.Instance.LoadConfig<ConfigActor>(Config.ActorConfig);
            AddComponent<GameObjectHolderComponent>();
            AddComponent<NumericComponent,ConfigCombatProperty[]>(configActor.Combat?.DefaultProperty);
            AddComponent<FsmComponent,ConfigFsmController>(ResourcesManager.Instance.LoadConfig<ConfigFsmController>(Config.FSM));
            AddComponent<CombatComponent,ConfigCombat>(configActor.Combat);
            using ListComponent<ConfigAbility> list = ConfigAbilityCategory.Instance.GetList(configActor.Abilities);
            AddComponent<AbilityComponent,List<ConfigAbility>>(list);
            if (!string.IsNullOrEmpty(monster.Config.AIPath))
            {
                AddComponent<MonsterAIInputComponent>();
                AddComponent<AIComponent,ConfigAIBeta>(ResourcesManager.Instance.LoadConfig<ConfigAIBeta>(monster.Config.AIPath));
            }
            if (!string.IsNullOrEmpty(monster.Config.PoseFSM))
            {
                AddComponent<PoseFSMComponent,ConfigFsmController>(ResourcesManager.Instance.LoadConfig<ConfigFsmController>(monster.Config.PoseFSM));
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