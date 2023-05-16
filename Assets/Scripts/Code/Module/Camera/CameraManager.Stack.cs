using System.Collections.Generic;

namespace TaoTie
{
    public partial class CameraManager: IUpdateComponent
    {
        
        #region CameraStack
        
        private int defaultCameraId;

        private Dictionary<int, ConfigCamera> configs;
        private Dictionary<int, CameraState> states;

        private PriorityStack<CameraState> cameraStack;

        private CameraState curCameraState;
        private partial void AfterInit()
        {
            var config = ResourcesManager.Instance.LoadConfig<ConfigCameras>("");
            defaultCameraId = config.DefaultCamera.Id;
            configs = new Dictionary<int, ConfigCamera>();
            configs.Add(config.DefaultCamera.Id,config.DefaultCamera);
            if (config.Cameras != null)
            {
                for (int i = 0; i < config.Cameras.Length; i++)
                {
                    configs.Add(config.Cameras[i].Id,config.Cameras[i]);
                }
            }

            states = new Dictionary<int, CameraState>();
            cameraStack = new PriorityStack<CameraState>();
            
            curCameraState = CreateCameraState(config.DefaultCamera);
            states[defaultCameraId] = curCameraState;
            cameraStack.Push(curCameraState);
        }
        
        public void Update()
        {
            foreach (var item in cameraStack)
            {
                item.Update();
            }

            var top = cameraStack.Peek();
            if (curCameraState != top)//需要变换相机了
            {
                
            }
        }
        
        private NormalCameraState CreateCameraState(ConfigCamera config)
        {
            return NormalCameraState.Create(config);
        }
        
        private BlenderCameraState CreateCameraState(NormalCameraState from, NormalCameraState to,bool isEnter)
        {
            return BlenderCameraState.Create(from, to, isEnter);
        }
        
        /// <summary>
        /// 创建相机
        /// </summary>
        /// <param name="configId"></param>
        public void Create(int configId)
        {
            if (configs.TryGetValue(configId, out var config))
            {
                
            }
        }

        /// <summary>
        /// 销毁相机
        /// </summary>
        /// <param name="configId"></param>
        public void Remove(int configId)
        {
            if (states.TryGetValue(configId, out var state))
            {
                
            }
        }
        #endregion
    }
}