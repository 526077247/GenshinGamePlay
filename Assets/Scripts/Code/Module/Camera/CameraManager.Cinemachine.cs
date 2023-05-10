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
        private readonly Dictionary<int, CameraPlugin> cameraPlugins = new();
        private CinemachineBrain _cinemachineBrain;

        private int curCameraId { get; set; } = int.MinValue;
        private CameraType curCameraType => curCamera.type;

        private CameraPlugin curCamera => cameraPlugins[curCameraId];

        private CameraState curState { get; set; }
        private CameraState lastState { get; set; }

        public VirtualCameraPlugin DefaultCamera =>
            cameraPlugins[defaultCameraId] as VirtualCameraPlugin;

        partial void AddInputListener();
        partial void RemoveInputListener();

        public void Init()
        {
            ConfigCameras config = ResourcesManager.Instance.LoadConfig<ConfigCameras>("EditConfig/ConfigCameras");
            defaultCameraId = config.DefaultCamera.Id;
            configs = new Dictionary<int, ConfigCamera>();
            configs.Add(defaultCameraId,config.DefaultCamera);
            for (int i = 0; i < config.Cameras.Length; i++)
            {
                configs.Add(config.Cameras[i].Id,config.Cameras[i]);
            }

            defaultBlend = config.DefaultBlend.ToCinemachineBlendDefinition();
            customBlends = new CinemachineBlenderSettings();
            if (config.CustomSetting != null)
            {
                customBlends.m_CustomBlends = new CinemachineBlenderSettings.CustomBlend[config.CustomSetting.Length];
                for (int i = 0; i < config.CustomSetting.Length; i++)
                {
                    customBlends.m_CustomBlends[i] = new CinemachineBlenderSettings.CustomBlend()
                    {
                        m_Blend = config.CustomSetting[i].Definition.ToCinemachineBlendDefinition()
                    };
                }
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
            foreach (var item in cameraPlugins) item.Value.OnRelease();

            cameraPlugins.Clear();

            sceneMainCameraGo = null;
            sceneMainCamera = null;
            curState = null;
            lastState = null;
            Instance = null;
            Object.Destroy(root);
        }

        public void Update()
        {
            if (cameraPlugins.TryGetValue(curCameraId, out var plugin))
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
                if (cameraPlugins.TryGetValue(curCameraId, out var plugin))
                {
                    plugin.OnLevel(true);
                }

                curCameraId = defaultCameraId;
                curCamera.OnEnter();
                _cinemachineBrain.m_DefaultBlend = defaultBlend;
                curState = new CameraState()
                {
                    Id = curCameraId,
                    Data = curCamera.stateData
                };
            }
        }

        private void SetCurCameraId(int id, bool clearState)
        {
            if (!cameraPlugins.ContainsKey(id))
            {
                CreateCamera(id);
            }

            int oldId = curCameraId;
            curCamera.OnLevel(clearState);
            curCameraId = id;
            curCamera.OnEnter();

            if (clearState && oldId!= defaultCameraId)
            {
                DestroyCamera(oldId);
            }
        }

        private CameraPlugin CreateCamera(ConfigCamera config)
        {
            if (cameraPlugins.ContainsKey(config.Id))
            {
                Log.Error("摄像机Id重复创建！Id=" + config.Id);
                return cameraPlugins[config.Id];
            }

            if (config is ConfigVirtualCamera configVirtualCamera)
            {
                var virtualCameraPlugin = new VirtualCameraPlugin();
                cameraPlugins[config.Id] = virtualCameraPlugin;
                virtualCameraPlugin.OnInit(configVirtualCamera);
                return virtualCameraPlugin;
            }

            if (config is ConfigFreeLookCamera configFreeLookCamera)
            {
                var freeLookCameraPlugin = new FreeLookCameraPlugin();
                cameraPlugins[config.Id] = freeLookCameraPlugin;
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

            if (cameraPlugins.TryGetValue(id, out var plugin))
            {
                plugin.OnRelease();
                cameraPlugins.Remove(id);
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
                Id = id,
                Data = curCamera.stateData
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
                var data = curState.Data as FreeLookCameraStateData;
                return data.Radius[1];
            }

            if (curCameraType == CameraType.VirtualCameraPlugin)
            {
                var data = curState.Data as VirtualCameraStateData;
                if (data?.Body == CinemachineBodyType.HardLockToTarget)
                    return -1;
                if (data?.Body == CinemachineBodyType.Transposer)
                    return data.Transposer.followOffset.z;
                if (data?.Body == CinemachineBodyType.FramingTransposer)
                    return data.FramingTransposer.cameraDistance;
                return -1;
            }

            return -1;
        }
    }
}