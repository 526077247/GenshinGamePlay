using UnityEngine;

namespace TaoTie
{
    public class CameraThirdPersonFollowPluginRunner: CameraBodyPluginRunner<ConfigCameraThirdPersonFollowPlugin>
    {
        private float angleOffsetX;
        private float angleOffsetY;
        private float distance;
        private ConfigEntityCommon entityCommon;
        private Vector3 position;
        private float wheel;
        private float mx;
        private float my;
        protected override void InitInternal()
        {
            angleOffsetX = 0;
            angleOffsetY = 0;
            distance = config.ZoomDefault;
            LoadCommonConfig();
            Calculating(false);
        }

        protected override void UpdateInternal()
        {
            CalculatingPara();
            Calculating(true);
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
            Calculating(false);
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
        
        private void Calculating(bool lerp)
        {
            if (state.follow != null && entityCommon != null)
            { 
                data.SphereQuaternion = Quaternion.Euler(new Vector3(angleOffsetY, angleOffsetX, 0));
                data.Forward = state.follow.forward;
                data.Up = state.follow.up;
                var newPosition = state.follow.position;
                if (lerp)
                {
                    newPosition = Vector3.Lerp(newPosition, position, 0.5f);
                }
                position = newPosition;
                
                data.Position = position - data.SphereQuaternion * Vector3.forward * distance +
                                data.Up * entityCommon.Height / 2;
            }
        }

        private void CalculatingPara()
        {
            #region 镜头缩放
            
            var newWheel = -InputManager.Instance.MouseScrollWheel;
            wheel = Mathf.Lerp(wheel, newWheel, 0.6f);
            distance += wheel * GameTimerManager.Instance.GetDeltaTime()/10f;
            distance = Mathf.Clamp(distance, config.ZoomMin, config.ZoomMax);
            
            #endregion

            #region 镜头旋转

            var newx = InputManager.Instance.MouseAxisX;
            mx = Mathf.Lerp(mx, newx, 0.6f);
            angleOffsetX += mx * GameTimerManager.Instance.GetDeltaTime()/200f * config.SpeedX;
            angleOffsetX %= 360;

            var newy = - InputManager.Instance.MouseAxisY;
            my = Mathf.Lerp(my, newy, 0.6f);
            angleOffsetY += my * GameTimerManager.Instance.GetDeltaTime()/200f * config.SpeedY;
            angleOffsetY = Mathf.Clamp(angleOffsetY, -60, 70);
            #endregion
        }
    }
}