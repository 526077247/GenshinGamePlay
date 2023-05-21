using UnityEngine;

namespace TaoTie
{
    public sealed class CameraThirdPersonLookAtPluginRunner: CameraHeadPluginRunner<ConfigCameraThirdPersonLookAtPlugin>
    {
        private ConfigEntityCommon entityCommon;

        protected override void InitInternal()
        {
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
        }
        
        public override void OnSetTarget()
        {
            base.OnSetTarget();
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
                    }
                }
            }
        }

        private void Calculating()
        {
            Vector3 dir;
            if (entityCommon != null)
            {
                dir = data.LookAt + data.TargetUp * entityCommon.Height / 2 - data.Position;
            }
            else
            {
                dir = data.LookAt - data.Position;
            }
            if (dir == Vector3.zero)
            {
                data.Orientation = Quaternion.LookRotation(data.TargetForward, data.TargetUp);
            }
            else
            {
                data.Orientation = Quaternion.LookRotation(dir);
                if (config.NearFocusEnable)
                {
                    var distance = dir.magnitude;
                    
                }
            }
        }
    }
}