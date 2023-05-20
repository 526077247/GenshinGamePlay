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

        private void Calculating()
        {
            if (state.follow != null)
            {
                data.Position = state.follow.position;
            }
        }
    }
}