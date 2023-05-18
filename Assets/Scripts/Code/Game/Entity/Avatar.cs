using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 玩家
    /// </summary>
    public class Avatar:Unit,IEntity<int>
    {
        private long thirdCameraId;
        #region IEntity

        public override EntityType Type => EntityType.Avatar;

        public void Init(int configId)
        {
            CampId = CampConst.Player;
            var avatar = AddComponent<AvatarComponent,int>(configId);
            ConfigId = avatar.Config.UnitId;
            ConfigEntity = ResourcesManager.Instance.LoadConfig<ConfigEntity>(Config.EntityConfig);
            AddComponent<GameObjectHolderComponent>();
            AddComponent<NumericComponent,ConfigCombatProperty[]>(ConfigEntity.Combat?.DefaultProperty);
            AddComponent<FsmComponent,ConfigFsmController>(ResourcesManager.Instance.LoadConfig<ConfigFsmController>(Config.FSM));
            AddComponent<CombatComponent>();
            AddComponent<SkillComponent>();
            AddComponent<LocalInputController>();
            AddComponent<AvatarMoveComponent>();
            using ListComponent<ConfigAbility> list = ConfigAbilityCategory.Instance.GetList(ConfigEntity.Abilities);
            AddComponent<AbilityComponent,List<ConfigAbility>>(list);
            InitAsync().Coroutine();
        }

        private async ETTask InitAsync()
        {
            var ghc = GetComponent<GameObjectHolderComponent>();
            await ghc.WaitLoadGameObjectOver();
            if (thirdCameraId == 0)
            {
                thirdCameraId = CameraManager.Instance.Create(GameConst.ThirdCameraConfigId);
            }
        }

        public void Destroy()
        {
            if (thirdCameraId != 0)
            {
                CameraManager.Instance.Remove(ref thirdCameraId);
            }
            ConfigEntity = null;
            ConfigId = default;
            CampId = 0;
        }

        #endregion
        
    }
}