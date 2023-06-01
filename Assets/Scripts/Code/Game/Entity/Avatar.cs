using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 玩家
    /// </summary>
    public class Avatar:Actor,IEntity<int>
    {
        private long thirdCameraId;
        #region IEntity

        public override EntityType Type => EntityType.Avatar;

        public void Init(int configId)
        {
            CampId = CampConst.Player;
            var avatar = AddComponent<AvatarComponent,int>(configId);
            ConfigId = avatar.Config.UnitId;
            configActor = ResourcesManager.Instance.LoadConfig<ConfigActor>(Config.ActorConfig);
            AddComponent<AttachComponent>();
            AddComponent<GameObjectHolderComponent>();
            AddComponent<NumericComponent,ConfigCombatProperty[]>(configActor.Combat?.DefaultProperty);
            AddComponent<FsmComponent,ConfigFsmController>(ResourcesManager.Instance.LoadConfig<ConfigFsmController>(Config.FSM));
            AddComponent<CombatComponent,ConfigCombat>(configActor.Combat);
            AddComponent<AvatarSkillComponent>();
            AddComponent<LocalInputController>();
            AddComponent<AvatarMoveComponent>();
            using ListComponent<ConfigAbility> list = ConfigAbilityCategory.Instance.GetList(configActor.Abilities);
            AddComponent<AbilityComponent,List<ConfigAbility>>(list);
            AddComponent<EquipHoldComponent>();
            InitAsync().Coroutine();
        }

        private async ETTask InitAsync()
        {
            var ghc = GetComponent<GameObjectHolderComponent>();
            await ghc.WaitLoadGameObjectOver();
            if(ghc.IsDispose) return;
            if (thirdCameraId == 0)
            {
                thirdCameraId = CameraManager.Instance.Create(GameConst.ThirdCameraConfigId);
            }

            var camera = CameraManager.Instance.Get<NormalCameraState>(thirdCameraId);
            var trans = ghc.EntityView;
            camera.SetFollow(trans);
            camera.SetTarget(trans);
        }

        public void Destroy()
        {
            if (thirdCameraId != 0)
            {
                CameraManager.Instance.Remove(ref thirdCameraId);
            }
            configActor = null;
            ConfigId = default;
            CampId = 0;
        }

        #endregion
        
    }
}