using System;
using System.Collections.Generic;

namespace TaoTie
{
    public partial class CameraManager: IUpdateComponent
    {
        
        #region CameraStack

        private Dictionary<Type, Type> configRunnerType;

        private int defaultCameraId;

        private Dictionary<int, ConfigCamera> configs;
        private Dictionary<long, CameraState> states;

        private PriorityStack<CameraState> cameraStack;

        private CameraState curCameraState;

        public ConfigCameraBlender defaultBlend;
        
        private partial void AfterInit()
        {
            #region Config

            var config = ResourcesManager.Instance.LoadConfig<ConfigCameras>("EditConfig/ConfigCameras");
            defaultBlend = config.DefaultBlend;
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
            #endregion

            #region RunnerType

            configRunnerType = new Dictionary<Type, Type>();
            var allTypes = AssemblyManager.Instance.GetTypes();
            var runnerType = TypeInfo<CameraPluginRunner>.Type;
            foreach (var item in allTypes)
            {
                var type = item.Value;
                if (!type.IsAbstract && runnerType.IsAssignableFrom(type))
                {
                    configRunnerType.Add(type.BaseType.GenericTypeArguments[0],type);
                }
            }

            #endregion
            states = new Dictionary<long, CameraState>();
            cameraStack = new PriorityStack<CameraState>();
            
            curCameraState = CreateCameraState(config.DefaultCamera, 0);
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
                    else//变换时，目标机位改变，需要变换到新的机位
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

            if (curCameraState != null)
            {
                ApplyData(curCameraState.Data);
            }
        }

        private void ApplyData(CameraStateData data)
        {
            sceneMainCamera.fieldOfView = data.Fov;
            //todo: 
        }

        private NormalCameraState CreateCameraState(ConfigCamera config, int priority)
        {
            var state = NormalCameraState.Create(config, priority);
            states.Add(state.Id,state);
            return state;
        }

        private BlenderCameraState CreateCameraState(NormalCameraState from, NormalCameraState to, bool isEnter)
        {
            var state = BlenderCameraState.Create(from, to, isEnter);
            states.Add(state.Id,state);
            return state;
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
        public void Remove(ref long id)
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

            id = 0;
        }

        /// <summary>
        /// 获取机位
        /// </summary>
        /// <param name="id"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>(long id) where T :CameraState
        {
            if (states.TryGetValue(id, out var data))
            {
                return data as T;
            }

            return null;
        }

        /// <summary>
        /// 从索引中删除，请不要手动调用
        /// </summary>
        public void RemoveState(long id)
        {
            states.Remove(id);
        }
        
        /// <summary>
        /// 创建Runner，请不要手动调用
        /// </summary>
        /// <param name="config"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public CameraPluginRunner CreatePluginRunner<T>(T config)where T :ConfigCameraPlugin
        {
            if(configRunnerType.TryGetValue(config.GetType(), out var runnerType))
            {
                var res = ObjectPool.Instance.Fetch(runnerType) as CameraPluginRunner;
                res.Init(config);
                return res;
            }
            Log.Error($"CreatePluginRunner失败， 未实现{config.GetType()}对应的Runner");
            return null;
        }
        
        
        #endregion
    }
}