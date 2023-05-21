using UnityEngine;

namespace TaoTie
{
    public sealed class CameraHardLookAtPluginRunner: CameraHeadPluginRunner<ConfigCameraHardLookAtPlugin>
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
        
        public override void OnSetTarget()
        {
            base.OnSetTarget();
            Calculating();
        }

        private void Calculating()
        {
            var dir = data.LookAt - data.Position;
            if (dir == Vector3.zero)
            {
                data.Orientation = Quaternion.LookRotation(data.TargetForward,data.TargetUp);
            }
            else
            {
                data.Orientation = Quaternion.LookRotation(dir);
            }
        }
    }
}