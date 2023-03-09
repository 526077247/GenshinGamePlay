using LitJson;
using UnityEngine;

namespace TaoTie
{
    public abstract class CameraStateData
    {
        public float dutch;
        public float farClipPlane;
        public float fov;
        public float nearClipPlane;
        public bool visibleCursor;
        public bool cameraShake;
        public bool enableZoom;
        public float zoomMin;
        public float zoomMax;
        public CursorLockMode mode;
        public CameraStateData()
        {
        }

        public CameraStateData(ConfigCamera config)
        {
            fov = config.fov;
            dutch = config.dutch;
            farClipPlane = config.farClipPlane;
            nearClipPlane = config.nearClipPlane;
            visibleCursor = config.visibleCursor;
            cameraShake = config.cameraShake;
            enableZoom = config.enableZoom;
            zoomMin = config.zoomMin;
            zoomMax = config.zoomMax;
            mode = config.mode;
        }

        public CameraStateData Clone()
        {
            var text = JsonMapper.ToJson(this);
            return JsonMapper.ToObject<CameraStateData>(text);
        }
    }
}