using UnityEngine;

namespace TaoTie
{
    public class CameraThirdPersonFollowPluginRunner: CameraBodyPluginRunner<ConfigCameraThirdPersonFollowPlugin>
    {
        private float angleOffsetX;
        private float angleOffsetY;
        protected override void InitInternal()
        {
            angleOffsetX = 0;
            angleOffsetY = 0;
            Calculating();
        }

        protected override void UpdateInternal()
        {
            Calculating();
        }

        protected override void DisposeInternal()
        {
            
        }
        
        public override void OnSetFollow()
        {
            base.OnSetFollow();
            Calculating();
        }

        private void Calculating()
        {
            if (state.follow != null)
            {
                data.Forward = state.follow.forward;
                data.Up = state.follow.up;
                
                data.Position = state.follow.position - config.ZoomDefault * data.Forward;
            }
        }
    }
}