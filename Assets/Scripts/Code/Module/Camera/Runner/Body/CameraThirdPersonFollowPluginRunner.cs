using UnityEngine;

namespace TaoTie
{
    public class CameraThirdPersonFollowPluginRunner: CameraBodyPluginRunner<ConfigCameraThirdPersonFollowPlugin>
    {
        private float angleOffsetX;
        private float angleOffsetY;
        private float distance;
        private ConfigActorCommon _actorCommon;
        private float wheel;
        private float mx;
        private float my;
        protected override void InitInternal()
        {
            angleOffsetX = 0;
            angleOffsetY = 0;
            distance = config.ZoomDefault;
            LoadCommonConfig();
            Calculating();
        }

        protected override void UpdateInternal()
        {
            CalculatingPara();
            Calculating();
        }

        protected override void DisposeInternal()
        {
            _actorCommon = null;
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
            _actorCommon = null;
            if (state.follow != null)
            {
                var ec = state.follow.GetComponent<EntityComponent>();
                if (ec != null)
                {
                    var entityId = ec.Id;
                    if (SceneManager.Instance.CurrentScene is BaseMapScene map)
                    {
                        var unit = map.GetManager<EntityManager>().Get<Actor>(entityId);
                        _actorCommon = unit.configActor.Common;
                    }
                }
            }
        }
        
        private void Calculating()
        {
            if (state.follow != null && _actorCommon != null)
            { 
                data.SphereQuaternion = Quaternion.Euler(new Vector3(angleOffsetY, angleOffsetX, 0));
                data.Forward = state.follow.forward;
                data.Up = state.follow.up;

                data.Position = state.follow.position - data.SphereQuaternion * Vector3.forward * distance +
                                data.Up * _actorCommon.Height / 2;
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