using UnityEngine;
using UnityEngine.Rendering.Universal;
namespace TaoTie
{
    public class CameraManager:IManager
    {
        public static CameraManager Instance { get; private set; }
        private GameObject sceneMainCameraGo;
        private Camera sceneMainCamera;
        #region override

        public void Init()
        {
            Instance = this;
        }


        public void Destroy()
        {
            Instance = null;
        }

        #endregion
        
        //在场景loading开始时设置camera statck
        //loading时场景被销毁，这个时候需要将UI摄像机从overlay->base
        public void SetCameraStackAtLoadingStart()
        {
            var ui_camera = UIManager.Instance.GetUICamera();
            ui_camera.GetUniversalAdditionalCameraData().renderType = CameraRenderType.Base;
            this.ResetSceneCamera();
        }

        public void  ResetSceneCamera()
        {
            this.sceneMainCameraGo = null;
            this.sceneMainCamera = null;
        }
        public void SetCameraStackAtLoadingDone()
        {
            this.sceneMainCameraGo = Camera.main.gameObject;
            this.sceneMainCamera = this.sceneMainCameraGo.GetComponent<Camera>();
            var render = this.sceneMainCamera.GetUniversalAdditionalCameraData();
            render.renderPostProcessing = true;
            render.renderType = CameraRenderType.Base;
            render.SetRenderer(1);
            var uiCamera = UIManager.Instance.GetUICamera();
            AddOverlayCamera(this.sceneMainCamera, uiCamera);
        }


        void AddOverlayCamera(Camera baseCamera, Camera overlayCamera)
        {
            overlayCamera.GetUniversalAdditionalCameraData().renderType = CameraRenderType.Overlay;
            baseCamera.GetUniversalAdditionalCameraData().cameraStack.Add(overlayCamera);
        }

        public Camera MainCamera()
        {
            return this.sceneMainCamera;
        }
    }
}