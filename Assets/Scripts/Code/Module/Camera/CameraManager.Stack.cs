using System;
using System.Collections.Generic;

namespace TaoTie
{
    public partial class CameraManager: IUpdateComponent
    {
        
        #region CameraStack
        
        private int defaultCameraId;

        private Dictionary<int, ConfigCamera> configs;
        private Dictionary<long, CameraState> states;

        private PriorityStack<CameraState> cameraStack;

        private CameraState curCameraState;

        private bool isUpdate;
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

            states = new Dictionary<long, CameraState>();
            cameraStack = new PriorityStack<CameraState>();
            
            curCameraState = CreateCameraState(config.DefaultCamera, 0);
            states[defaultCameraId] = curCameraState;
            cameraStack.Push(curCameraState);
        }
        
        public void Update()
        {
            isUpdate = true;
            foreach (var item in cameraStack)
            {
                item.Update();
            }
            isUpdate = false;
            var top = cameraStack.Peek();
            if (curCameraState != top)//栈顶相机机位变更，需要变换
            {
                if (top is BlenderCameraState blender) //正在变换
                {
                    cameraStack.Pop();
                    while (cameraStack.Peek().IsOver) //移除已经over的
                    {
                        cameraStack.Pop().Dispose();
                    }

                    var newTop = cameraStack.Peek();
                    blender.ChangeTo(newTop as NormalCameraState, false);
                    cameraStack.Push(blender);
                }
                else//变换到下一个相机机位
                {
                    while (cameraStack.Peek().IsOver)//移除已经over的
                    {
                        cameraStack.Pop().Dispose();
                    }
                    blender = CreateCameraState(curCameraState as NormalCameraState, cameraStack.Peek() as NormalCameraState,
                        false);
                    cameraStack.Push(blender);
                    curCameraState = blender;
                }
            }
            else if(top.IsOver)//播放完毕，需要变换相机机位
            {
                if (top is BlenderCameraState blender)//正在变换
                {
                    cameraStack.Pop();
                    while (cameraStack.Peek().IsOver)//移除已经over的
                    {
                        cameraStack.Pop().Dispose();
                    }

                    var newTop = cameraStack.Peek();
                    if (blender.To.Id == newTop.Id)//是变换完成了
                    {
                        curCameraState = cameraStack.Peek();
                        top.Dispose();
                    }
                    else//变换时，目标机位被销毁，需要变换到新的机位
                    {
                        blender.ChangeTo(newTop as NormalCameraState, false);
                        cameraStack.Push(blender);
                    }
                }
                else//一般相机被销毁，需要出栈，变换到下一个相机机位
                {
                    cameraStack.Pop();
                    while (cameraStack.Peek().IsOver)//移除已经over的
                    {
                        cameraStack.Pop().Dispose();
                    }
                    blender = CreateCameraState(top as NormalCameraState, cameraStack.Peek() as NormalCameraState,
                        false);
                    cameraStack.Push(blender);
                    curCameraState = blender;
                }
            }
        }

        private NormalCameraState CreateCameraState(ConfigCamera config, int priority)
        {
            return NormalCameraState.Create(config, priority);
        }

        private BlenderCameraState CreateCameraState(NormalCameraState from, NormalCameraState to, bool isEnter)
        {
            return BlenderCameraState.Create(from, to, isEnter);
        }

        /// <summary>
        /// 创建相机
        /// </summary>
        /// <param name="configId"></param>
        /// <param name="priority"></param>
        public long Create(int configId, int priority = 0)
        {
            if (configs.TryGetValue(configId, out var config))
            {
                var res = CreateCameraState(config, priority);
                res.Priority = priority;
                cameraStack.Push(res);
                return res.Id;
            }
            Log.Error($"创建相机时 configId = {configId} 不存在");
            return 0;
        }

        /// <summary>
        /// 销毁相机
        /// </summary>
        /// <param name="id"></param>
        public void Remove(long id)
        {
            if(id == defaultCameraId) return;//默认相机不销毁

            if (states.TryGetValue(id, out var state))
            {
                state.IsOver = true;
                
                //非生效相机
                if (curCameraState is BlenderCameraState blender)
                {
                    if (blender.To.Id != state.Id)
                    {
                        cameraStack.Remove(state);
                        state.Dispose();
                    }
                }
                else if(curCameraState.Id != state.Id)
                {
                    cameraStack.Remove(state);
                    state.Dispose();
                }
            }
        }
        #endregion
    }
}