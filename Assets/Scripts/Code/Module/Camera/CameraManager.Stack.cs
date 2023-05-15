using System.Collections.Generic;

namespace TaoTie
{
    public partial class CameraManager: IUpdateComponent
    {
        
        #region CameraStack

        private int defaultCameraId;

        private Dictionary<int, ConfigCamera> configs;

        private PriorityStack<CameraState> cameraStack;

        private partial void AfterInit()
        {
            cameraStack = new PriorityStack<CameraState>();
        }
        
        public void Update()
        {
            foreach (var item in cameraStack)
            {
                item.Update();
            }
        }

        #endregion
    }
}