using UnityEngine;

namespace TaoTie
{
    public sealed class CameraColliderPluginRunner: CameraOtherPluginRunner<ConfigCameraColliderPlugin>
    {
        protected override void InitInternal()
        {
            
        }

        protected override void DisposeInternal()
        {
            
        }

        protected override void UpdateInternal()
        {
            if (PhysicsHelper.SphereCast(data.LookAt,data.Position,config.Radius,config.CastLayer,out var hit))
            {
                data.Position = hit.point + hit.normal * config.Radius;
            }
        }
    }
}