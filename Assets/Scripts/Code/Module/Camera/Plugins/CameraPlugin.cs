using Cinemachine;
using UnityEngine;

namespace TaoTie
{
    public abstract class CameraPlugin
    {
        protected CinemachineVirtualCameraBase baseCamera;
        protected ConfigCamera config;
        protected GameObject obj;
        public int id => config.id;
        public abstract CameraType type { get; }
        public CameraStateData stateData { get; protected set; }
        public abstract void OnInit(ConfigCamera config);
        public abstract void ChangeState(CameraStateData state);

        public virtual void OnEnter()
        {
            baseCamera.Priority = 999;
        }

        public virtual void OnLevel(bool clearState)
        {
            baseCamera.Priority = 0;
        }

        public virtual void Tick()
        {
            if (stateData != null)
            {
                Cursor.visible = stateData.visibleCursor;
                Cursor.lockState = stateData.mode;
            }
        }

        public abstract void OnRelease();


        protected virtual void SetFollowTransform(Transform transform)
        {
            baseCamera.Follow = transform;
        }

        protected virtual void SetLookAtTransform(Transform transform)
        {
            baseCamera.LookAt = transform;
        }
    }

    public abstract class CameraPlugin<T, V> : CameraPlugin where T : ConfigCamera where V : CameraStateData
    {
        public CameraShakeListener _shakeListener;
        public T defaultConfig => config as T;

        public sealed override void OnInit(ConfigCamera config)
        {
            OnInitInternal(config as T);
        }

        protected virtual void OnInitInternal(T data)
        {
            config = data;
            obj = new GameObject(type.ToString() + id);
            obj.transform.parent = CameraManager.Instance.root;
        }

        public override void OnRelease()
        {
            _shakeListener = null;
            config = null;
            stateData = null;
            Object.Destroy(obj);
        }


        public sealed override void ChangeState(CameraStateData state)
        {
            var data = state as V;
            if (data != null)
                ChangeStateInternal(data);
        }

        protected virtual void ChangeStateInternal(V state)
        {
            stateData = state;
            Cursor.visible = state.visibleCursor;
            Cursor.lockState = state.mode;
            if (stateData.cameraShake)
            {
                if (_shakeListener == null)
                {
                    _shakeListener = obj.AddComponent<CameraShakeListener>();
                    baseCamera.AddExtension(_shakeListener);
                }

                _shakeListener.enabled = true;
            }
            else
            {
                if (_shakeListener != null)
                    _shakeListener.enabled = false;
            }
        }

        public override void OnLevel(bool clearState)
        {
            base.OnLevel(clearState);
            if (clearState)
                stateData = null;
        }
    }
}