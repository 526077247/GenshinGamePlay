using UnityEngine;

namespace TaoTie
{
    public class CameraThirdPersonFollowPluginRunner: CameraBodyPluginRunner<ConfigCameraThirdPersonFollowPlugin>
    {
        private float angleOffsetX;
        private float angleOffsetY;
        private ConfigEntityCommon entityCommon;
        protected override void InitInternal()
        {
            angleOffsetX = 0;
            angleOffsetY = 0;
            LoadCommonConfig();
            Calculating();
        }

        protected override void UpdateInternal()
        {
            Calculating();
        }

        protected override void DisposeInternal()
        {
            entityCommon = null;
            angleOffsetX = default;
            angleOffsetY = default;
        }
        
        public override void OnSetFollow()
        {
            base.OnSetFollow();
            LoadCommonConfig();
            Calculating();
        }

        
        private void LoadCommonConfig()
        {
            entityCommon = null;
            if (state.follow != null)
            {
                var ec = state.follow.GetComponent<EntityComponent>();
                if (ec != null)
                {
                    var entityId = ec.Id;
                    if (SceneManager.Instance.CurrentScene is BaseMapScene map)
                    {
                        var unit = map.GetManager<EntityManager>().Get<Unit>(entityId);
                        entityCommon = unit.ConfigEntity.Common;
                        return;
                    }
                }
            }
            Log.Error("LoadCommonConfig 未找到 ConfigEntity.Common 配置");
        }
        
        private void Calculating()
        {
            if (state.follow != null && entityCommon != null)
            {
                data.Forward = state.follow.forward;
                data.Up = state.follow.up;
                
                data.Position = state.follow.position - 
                                config.ZoomDefault * data.Forward + 
                                data.Up * entityCommon.Height / 2;
            }
        }
    }
}