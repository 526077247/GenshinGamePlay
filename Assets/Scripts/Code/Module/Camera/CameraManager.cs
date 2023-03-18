using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Cinemachine;
namespace TaoTie
{
    public partial class CameraManager:IManager,IUpdateManager
    {
        #region config

        private int defaultCameraId;

        private Dictionary<int, ConfigCamera> configs;
        private CinemachineBlendDefinition defaultBlend;
        private CinemachineBlenderSettings customBlends;
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
            else if (sceneMainCameraGo == null) //场景没有主摄像机且没有创建摄像机
            {
                sceneMainCameraGo = new GameObject("MainCamera");
                sceneMainCameraGo.transform.parent = root;
                sceneMainCameraGo.tag = "MainCamera";
                sceneMainCamera = sceneMainCameraGo.AddComponent<Camera>();
            }
            var render = this.sceneMainCamera.GetUniversalAdditionalCameraData();
            render.renderPostProcessing = true;
            render.renderType = CameraRenderType.Base;
            render.SetRenderer(1);
            var uiCamera = UIManager.Instance.GetUICamera();
            AddOverlayCamera(this.sceneMainCamera, uiCamera);
            SetCameraAtLoadingDone();
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