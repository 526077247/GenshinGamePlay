using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace TaoTie
{
    public partial class CameraManager:IManager
    {
        #region IManager

        public void Init()
        {
            Instance = this;
        }

        public void Destroy()
        {
            Instance = null;
        }

        #endregion
        #region config

        private int defaultCameraId;

        private Dictionary<int, ConfigCamera> configs;

        #endregion
         
        public static CameraManager Instance { get; private set; }
        private GameObject sceneMainCameraGo;
        private Camera sceneMainCamera;
        
        
        //在场景loading开始时设置camera statck
        //loading时场景被销毁，这个时候需要将UI摄像机从overlay->base
        public void SetCameraStackAtLoadingStart()
        {
            var uiCamera = UIManager.Instance.GetUICamera();
            uiCamera.GetUniversalAdditionalCameraData().renderType = CameraRenderType.Base;
            this.ResetSceneCamera();
        }

        public void  ResetSceneCamera()
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