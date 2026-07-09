using UnityEngine;

namespace TaoTie
{
    public partial class CameraManager
    {
        #region URPCamera
        
        private GameObject sceneMainCameraGo;
        private Camera sceneMainCamera;
        
        //在场景loading开始时设置camera statck
        //loading时场景被销毁，这个时候需要将UI摄像机从overlay->base
        public void SetCameraStackAtLoadingStart()
        {
            this.ResetSceneCamera();
        }

        public void ResetSceneCamera()
        {
            this.sceneMainCameraGo = null;
            this.sceneMainCamera = null;
        }
        public void SetCameraStackAtLoadingDone()
        {
            var mainCamera = Camera.main;
            if (mainCamera != null) //场景已有主摄像机
            {
                if (sceneMainCamera != null)
                {
                    sceneMainCamera = null;
                    Object.Destroy(sceneMainCameraGo);
                }

                sceneMainCamera = mainCamera;
                sceneMainCameraGo = sceneMainCamera.gameObject;
            }
        }

        public Camera MainCamera()
        {
            return this.sceneMainCamera;
        }
        
        #endregion
    }
}