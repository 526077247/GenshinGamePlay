using UnityEngine;
using UnityEngine.Rendering.Universal;
namespace TaoTie
{
    public class CameraManager:IManager
    {
        public static CameraManager Instance;
        public GameObject m_scene_main_camera_go;
        public Camera m_scene_main_camera;
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
            this.m_scene_main_camera_go = null;
            this.m_scene_main_camera = null;
        }
        public void SetCameraStackAtLoadingDone()
        {
            this.m_scene_main_camera_go = Camera.main.gameObject;
            this.m_scene_main_camera = this.m_scene_main_camera_go.GetComponent<Camera>();
            var render = this.m_scene_main_camera.GetUniversalAdditionalCameraData();
            render.renderPostProcessing = true;
            render.renderType = CameraRenderType.Base;
            render.SetRenderer(1);
            var ui_camera = UIManager.Instance.GetUICamera();
            __AddOverlayCamera(this.m_scene_main_camera, ui_camera);
        }


        void __AddOverlayCamera(Camera baseCamera, Camera overlayCamera)
        {
            overlayCamera.GetUniversalAdditionalCameraData().renderType = CameraRenderType.Overlay;
            baseCamera.GetUniversalAdditionalCameraData().cameraStack.Add(overlayCamera);
        }
    }
}