using UnityEngine;

namespace TaoTie
{
    public sealed class CameraHardLockToTargetPluginRunner: CameraBodyPluginRunner<ConfigCameraHardLockToTargetPlugin>
    {
        protected override void InitInternal()
        {
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
                data.SphereQuaternion = state.follow.rotation;
                data.Position = state.follow.position;
            }
        }
    }
}