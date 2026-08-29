using UnityEngine;

namespace TaoTie
{
    public class CameraThirdPersonFollowPluginRunner: CameraBodyPluginRunner<ConfigCameraThirdPersonFollowPlugin>
    {
        private float angleOffsetX;
        private float angleOffsetY;
        private float distance;
        private ConfigActorCommon actorCommon;
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
            if (state.IsCurrentCamera)
            {
                CalculatingPara(CameraManager.Instance.Input.Current);
            }
            Calculating();
        }

        protected override void DisposeInternal()
        {
            actorCommon = null;
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
            actorCommon = null;
            if (state.follow != null)
            {
                if (state.follow != null)
                {
                    var actor = state.follow as Actor;
                    if (actor != null)
                    {
                        actorCommon = actor.ConfigActor.Common;
                    }
                }
            }
        }
        
        private void Calculating()
        {
            if (state.follow != null && actorCommon != null)
            { 
                data.SphereQuaternion = Quaternion.Euler(new Vector3(angleOffsetY, angleOffsetX, 0));
                data.Forward = state.follow.Forward;
                data.Up = state.follow.Up;

                data.Position = state.follow.Position - data.SphereQuaternion * Vector3.forward * distance +
                                data.Up * actorCommon.Height / 2;
            }
        }

        private void CalculatingPara(CameraInputIntent input)
        {
            #region 镜头缩放
            
            var newWheel = -input.ScrollDelta;
            wheel = Mathf.Lerp(wheel, newWheel, 0.6f);
            distance += wheel * GameTimerManager.Instance.GetDeltaTime()/10f;
            distance = Mathf.Clamp(distance, config.ZoomMin, config.ZoomMax);
            
            #endregion

            #region 镜头旋转
            if (input.IsCursorUnLocked) return;
            var newx = input.LookDelta.x;
            mx = Mathf.Lerp(mx, newx, 0.6f);
            angleOffsetX += mx * GameTimerManager.Instance.GetDeltaTime()/200f * config.SpeedX;
            angleOffsetX %= 360;

            var newy = - input.LookDelta.y;
            my = Mathf.Lerp(my, newy, 0.6f);
            angleOffsetY += my * GameTimerManager.Instance.GetDeltaTime()/200f * config.SpeedY;
            angleOffsetY = Mathf.Clamp(angleOffsetY, -60, 70);
            #endregion
        }
    }
}