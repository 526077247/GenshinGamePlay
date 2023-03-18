using System;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TaoTie
{
    public partial class CameraManager
    {
        public Transform root;
        private readonly Dictionary<int, CameraPlugin> _cameraPlugins = new();
        private CinemachineBrain _cinemachineBrain;

        public int curCameraId { get; private set; } = int.MinValue;
        public CameraType curCameraType => curCamera.type;

        public CameraPlugin curCamera => _cameraPlugins[curCameraId];

        public CameraState curState { get; private set; }
        public CameraState lastState { get; private set; }

        public VirtualCameraPlugin defaultCamera =>
            _cameraPlugins[defaultCameraId] as VirtualCameraPlugin;

        partial void AddInputListener();
        partial void RemoveInputListener();

        public void Init()
        {
            ConfigCameras config = ResourcesManager.Instance.LoadConfig<ConfigCameras>("EditConfig/ConfigCameras.bytes");
            defaultCameraId = config.defaultCamera.id;
            configs = new Dictionary<int, ConfigCamera>();
            configs.Add(defaultCameraId,config.defaultCamera);
            for (int i = 0; i < config.cameras.Length; i++)
            {
                configs.Add(config.cameras[i].id,config.cameras[i]);
            }

            defaultBlend = config.defaultBlend.ToCinemachineBlendDefinition();
            customBlends = new CinemachineBlenderSettings();
            customBlends.m_CustomBlends = new CinemachineBlenderSettings.CustomBlend[config.customSetting.Length];
            for (int i = 0; i < config.customSetting.Length; i++)
            {
                customBlends.m_CustomBlends[i] = new CinemachineBlenderSettings.CustomBlend()
                {
                    m_Blend = config.customSetting[i].definition.ToCinemachineBlendDefinition()
                };
            }
            
            root = new GameObject("CameraRoot").transform;
            Object.DontDestroyOnLoad(root);
            Instance = this;
            CreateCamera(defaultCameraId);
            AddInputListener();
        }

        public void Destroy()
        {
            RemoveInputListener();
            foreach (var item in _cameraPlugins) item.Value.OnRelease();

            _cameraPlugins.Clear();

            sceneMainCameraGo = null;
            sceneMainCamera = null;
            curState = null;
            lastState = null;
            Instance = null;
            Object.Destroy(root);
        }

        public void Update()
        {
            if (_cameraPlugins.TryGetValue(curCameraId, out var plugin))
            {
                plugin.Tick();
            }
        }

        private void SetCameraAtLoadingDone()
        {
            _cinemachineBrain = sceneMainCameraGo.GetComponent<CinemachineBrain>();
            if (_cinemachineBrain == null) _cinemachineBrain = sceneMainCameraGo.AddComponent<CinemachineBrain>();
            _cinemachineBrain.m_DefaultBlend = defaultBlend;
            _cinemachineBrain.m_CustomBlends = customBlends;
            if (curCameraId != defaultCameraId)
            {
                _cinemachineBrain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.Cut;
                if (_cameraPlugins.TryGetValue(curCameraId, out var plugin))
                {
                    plugin.OnLevel(true);
                }

                curCameraId = defaultCameraId;
                curCamera.OnEnter();
                _cinemachineBrain.m_DefaultBlend = defaultBlend;
                curState = new CameraState()
                {
                    id = curCameraId,
                    data = curCamera.stateData
                };
            }
        }

        private void SetCurCameraId(int id, bool clearState)
        {
            if (!_cameraPlugins.ContainsKey(id))
            {
                CreateCamera(id);
            }

            int oldId = curCameraId;
            curCamera.OnLevel(clearState);
            curCameraId = id;
            curCamera.OnEnter();

            if (clearState)
            {
                DestroyCamera(oldId);
            }
        }

        private CameraPlugin CreateCamera(ConfigCamera config)
        {
            if (_cameraPlugins.ContainsKey(config.id))
            {
                Log.Error("摄像机Id重复创建！Id=" + config.id);
                return _cameraPlugins[config.id];
            }

            if (config is ConfigVirtualCamera configVirtualCamera)
            {
                var virtualCameraPlugin = new VirtualCameraPlugin();
                _cameraPlugins[config.id] = virtualCameraPlugin;
                virtualCameraPlugin.OnInit(configVirtualCamera);
                return virtualCameraPlugin;
            }

            if (config is ConfigFreeLookCamera configFreeLookCamera)
            {
                var freeLookCameraPlugin = new FreeLookCameraPlugin();
                _cameraPlugins[config.id] = freeLookCameraPlugin;
                freeLookCameraPlugin.OnInit(configFreeLookCamera);
                return freeLookCameraPlugin;
            }

            // if (config is ConfigTrackCamera configTrackCamera)
            // {
            //     var trackCameraPlugin = new TrackCameraPlugin();
            //     _cameraPlugins[config.id] = trackCameraPlugin;
            //     trackCameraPlugin.OnInit(configTrackCamera);
            //     return trackCameraPlugin;
            // }

            throw new Exception("指定相机功能未实现");
        }

        private CameraPlugin CreateCamera(int id)
        {
            if (configs.TryGetValue(id, out var config))
            {
                return CreateCamera(config);
            }

            Log.Error("指定相机未配置! id = " + id);
            return null;
        }

        public void DestroyCamera(int id)
        {
            if (id == defaultCameraId)
            {
                Log.Error("默认相机不能销毁");
                return;
            }

            if (curCameraId == id)
            {
                Log.Error("当前相机不能销毁");
                return;
            }

            if (_cameraPlugins.TryGetValue(id, out var plugin))
            {
                plugin.OnRelease();
                _cameraPlugins.Remove(id);
            }
        }

        /// <summary>
        ///     切换摄像机
        /// </summary>
        /// <param name="id"></param>
        /// <param name="data"></param>
        /// <param name="clearState">是否清除之前摄像机状态</param>
        public void ChangeCameraState(int id, CameraStateData data = null, bool clearState = true)
        {
            if (id != curCameraId) SetCurCameraId(id, clearState);

            if (data != null) curCamera.ChangeState(data);

            var state = new CameraState
            {
                id = id,
                data = curCamera.stateData
            };
            lastState = curState;
            curState = state;
        }

        /// <summary>
        ///     获取相机距离
        /// </summary>
        public float GetCurCameraDistance()
        {
            if (curCameraType == CameraType.FreeLookCameraPlugin)
            {
                var data = curState.data as FreeLookCameraStateData;
                return data.radius[1];
            }

            if (curCameraType == CameraType.VirtualCameraPlugin)
            {
                var data = curState.data as VirtualCameraStateData;
                if (data?.body == CinemachineBodyType.HardLockToTarget)
                    return -1;
                if (data?.body == CinemachineBodyType.Transposer)
                    return data.transposer.followOffset.z;
                if (data?.body == CinemachineBodyType.FramingTransposer)
                    return data.framingTransposer.cameraDistance;
                return -1;
            }

            return -1;
        }
    }
}