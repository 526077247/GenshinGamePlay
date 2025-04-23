using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 非玩家角色
    /// </summary>
    public class Character:Actor,IEntity<int,uint>
    {
        #region IEntity

        public override EntityType Type => EntityType.Avatar;

        public void Init(int configId,uint campId)
        {
            CampId = campId;
            var avatar = AddComponent<AvatarComponent,int>(configId);
            ConfigId = avatar.Config.UnitId;
            ConfigActor = GetActorConfig(Config.ActorConfig);
            if(ConfigActor.Common!=null) LocalScale = Vector3.one * ConfigActor.Common.Scale;
            AddComponent<AttachComponent>();
            AddComponent<UnitModelComponent,ConfigModel>(ConfigActor.Model);
            AddComponent<NumericComponent,ConfigCombatProperty[]>(ConfigActor.Combat?.DefaultProperty);
            AddComponent<FsmComponent,ConfigFsmController>(GetFsmConfig(Config.FSM));
            AddComponent<CombatComponent,ConfigCombat>(ConfigActor.Combat);
            AddComponent<SkillComponent>();
            AddComponent<ORCAAgentComponent>();
            using ListComponent<ConfigAbility> list = ConfigAbilityCategory.Instance.GetList(ConfigActor.Abilities);
            AddComponent<AbilityComponent,List<ConfigAbility>>(list);
            AddComponent<EquipHoldComponent>();
            CreateMoveComponent();
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