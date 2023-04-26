using LitJson;
using UnityEngine;

namespace TaoTie
{
    public abstract class CameraStateData
    {
        public float Dutch;
        public float FarClipPlane;
        public float Fov;
        public float NearClipPlane;
        public bool VisibleCursor;
        public bool CameraShake;
        public bool EnableZoom;
        public float ZoomMin;
        public float ZoomMax;
        public CursorLockMode Mode;
        public CameraStateData()
        {
        }

        public CameraStateData(ConfigCamera config)
        {
            Fov = config.Fov;
            Dutch = config.Dutch;
            FarClipPlane = config.FarClipPlane;
            NearClipPlane = config.NearClipPlane;
            VisibleCursor = config.VisibleCursor;
            CameraShake = config.CameraShake;
            EnableZoom = config.EnableZoom;
            ZoomMin = config.ZoomMin;
            ZoomMax = config.ZoomMax;
            Mode = config.Mode;
        }

        public CameraStateData Clone()
        {
            var text = JsonMapper.ToJson(this);
            return JsonMapper.ToObject<CameraStateData>(text);
        }
    }
}