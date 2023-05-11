using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 怪物
    /// </summary>
    public class Monster: Unit,IEntity<int,Vector3,uint>
    {
        #region IEntity
        
        public override EntityType Type => EntityType.Monster;
        
        public void Init(int configId,Vector3 bornPos,uint campId)
        {
            Position = bornPos;
            CampId = campId;
            var monster = AddComponent<MonsterComponent,int>(configId);
            ConfigId = monster.Config.UnitId;
            ConfigEntity = ResourcesManager.Instance.LoadConfig<ConfigEntity>(Config.EntityConfig);
            AddComponent<GameObjectHolderComponent>();
            AddComponent<NumericComponent,ConfigCombatProperty[]>(ConfigEntity.Combat?.DefaultProperty);
            AddComponent<FsmComponent,ConfigFsmController>(ResourcesManager.Instance.LoadConfig<ConfigFsmController>(Config.FSM));
            AddComponent<CombatComponent>();
            using ListComponent<ConfigAbility> list = ConfigAbilityCategory.Instance.GetList(ConfigEntity.Abilities);
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