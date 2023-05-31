namespace TaoTie
{
    public class BillboardHpPlugin: BillboardPlugin<ConfigBillboardHpPlugin>
    {
        #region BillboardPlugin

        protected override void InitInternal()
        {
            
        }

        protected override void UpdateInternal()
        {
            if (obj != null)
            {
                this.obj.transform.rotation = CameraManager.Instance.MainCamera().transform.rotation;
            }
        }

        protected override void DisposeInternal()
        {
            
        }

        protected override void OnGameObjectLoaded()
        {
            this.obj.transform.rotation = CameraManager.Instance.MainCamera().transform.rotation;
        }

        #endregion
    }
}