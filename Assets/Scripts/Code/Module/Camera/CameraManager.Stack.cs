using System;
using System.Collections.Generic;
#if RoslynAnalyzer
using Unity.Code.NinoGen;
#endif
using UnityEngine;

namespace TaoTie
{
    public partial class CameraManager: IUpdate
    {
        
        #region CameraStack

        private Dictionary<Type, Type> configRunnerType;

        private int defaultCameraId;

        private Dictionary<int, ConfigCamera> configs;
        private Dictionary<long, CameraState> states;

        private PriorityStack<CameraState> cameraStack;

        private CameraState curCameraState;

        public ConfigBlender DefaultBlend { get; private set; }
        public CameraState CurrentCameraState => curCameraState;
        public int CursorVisibleState { get; private set; }= 0;
        public int CursorLockState { get; private set; }= 0;

        private async ETTask<ConfigCameras> GetConfig(string path = "EditConfig/OthersBuildIn/ConfigCameras")
        {
            if (Define.ConfigType == 0)
            {
                var jStr = await ResourcesManager.Instance.LoadConfigJsonAsync(path);
                return JsonHelper.FromJson<ConfigCameras>(jStr);
            }
#if RoslynAnalyzer
            else
            {
                var bytes = await ResourcesManager.Instance.LoadConfigBytesAsync(path);
                Deserializer.Deserialize(bytes,out ConfigCameras res);
                return res;
            }
#endif
            Log.Error($"GetConfig 失败，ConfigType = {Define.ConfigType} 未处理");
            return null;
        }
        public async partial ETTask LoadAsync()
        {
            #region Config
            var config = await GetConfig("EditConfig/OthersBuildIn/ConfigCameras");
            DefaultBlend = config.DefaultBlend;
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
            
            SetCurCameraState(CreateCameraState(config.DefaultCamera, 0));
            states[defaultCameraId] = curCameraState;
            cameraStack.Push(curCameraState);
        }
        
        public void Update()
        {
            if (cameraStack == null) return;
            foreach (var item in cameraStack.Data)
            {
                if(item.Value == null) continue;
                for (int i = 0; i < item.Value.Count; i++)
                {
                    item.Value[i]?.Update();
                }
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
                        true);
                    cameraStack.Push(blender);
                    SetCurCameraState(blender);
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
                        SetCurCameraState(cameraStack.Peek());
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
                    SetCurCameraState(blender);
                }
            }

            if (curCameraState != null)
            {
                ApplyData(curCameraState.Data);
            }

            if (InputManager.Instance == null) return;
            if (InputManager.Instance.GetKeyDown(GameKeyCode.CursorUnlock))
            {
                ChangeCursorLock(true, CursorStateType.UserInput);
                ChangeCursorVisible(true, CursorStateType.UserInput);
            }
            else if (InputManager.Instance.GetAnyKeyDownExcept(GameKeyCode.CursorUnlock))
            {
                ChangeCursorLock(false, CursorStateType.UserInput);
                ChangeCursorVisible(false, CursorStateType.UserInput);
            }
        }

        private void ApplyData(CameraStateData data)
        {
            if (sceneMainCamera == null || data == null) return;
            sceneMainCamera.fieldOfView = data.Fov;
            sceneMainCamera.nearClipPlane = data.NearClipPlane;
            sceneMainCamera.gameObject.transform.rotation = data.Orientation;
            sceneMainCamera.gameObject.transform.position = data.Position;
            //todo: 
        }

        private void SetCurCameraState(CameraState state)
        {
            if(curCameraState!=null && !curCameraState.IsBackground) curCameraState.OnLeave();
            curCameraState = state;
            curCameraState.OnEnter();
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
            if (states.TryGetValue(id, out var state))
            {
                states.Remove(id);
            }
        }
        
        /// <summary>
        /// 创建Runner，请不要手动调用, 使用<see cref="Create"/>创建
        /// </summary>
        /// <param name="config"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public CameraPluginRunner CreatePluginRunner<T>(T config, NormalCameraState state)where T :ConfigCameraPlugin
        {
            if(configRunnerType.TryGetValue(config.GetType(), out var runnerType))
            {
                var res = ObjectPool.Instance.Fetch(runnerType) as CameraPluginRunner;
                res.Init(config,state);
                return res;
            }
            Log.Error($"CreatePluginRunner失败， 未实现{config.GetType()}对应的Runner");
            return null;
        }
        
        
        #endregion

        public void ChangeCursorVisible(bool visible, CursorStateType type)
        {
            int flag = (int) type;
            if (visible)
            {
                CursorVisibleState |= flag;
            }
            else if((CursorVisibleState&flag) != 0)
            {
                CursorVisibleState -= flag;
            }

            Cursor.visible = CursorVisibleState > 0;
        }
        public void ChangeCursorLock(bool isUnLock, CursorStateType type)
        {
            int flag = (int) type;
            if (isUnLock)
            {
                CursorLockState |= flag;
            }
            else if((CursorLockState&flag) != 0)
            {
                CursorLockState -= flag;
            }
            
            Cursor.lockState = CursorLockState > 0? CursorLockMode.None: CursorLockMode.Locked;
        }
        public void ResetCursorState()
        {
            CursorVisibleState = 0;
            CursorLockState = 0;
            if (curCameraState is BlenderCameraState blender)
            {
                ChangeCursorLock(blender.To.Config.UnLockCursor, CursorStateType.Camera);
                ChangeCursorVisible(blender.To.Config.VisibleCursor, CursorStateType.Camera);
            }
            else if (curCameraState is NormalCameraState normal)
            {
                ChangeCursorLock(normal.Config.UnLockCursor, CursorStateType.Camera);
                ChangeCursorVisible(normal.Config.VisibleCursor, CursorStateType.Camera);
            }
        }
    }
}