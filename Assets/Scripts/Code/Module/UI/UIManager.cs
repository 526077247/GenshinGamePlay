using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// <p>fd: UI管理类，所有UI都应该通过该管理类进行创建 </p>
    /// <p>如打开UI窗口 <see cref="OpenWindow{P,T1}"/></p>
    /// <p>提供UI操作、UI层级、UI消息、UI资源加载、UI调度、UI缓存等管理</p>
    /// </summary>
    public partial class UIManager : IManager
    {

        public static UIManager Instance { get; private set; }

        private Dictionary<string, UIWindow> windows; //所有存活的窗体  {uiName:window}
        private Dictionary<UILayerNames, LinkedList<string>> windowStack; //窗口记录队列

        public float ScreenSizeFlag
        {
            get
            {
                float width = Screen.width;
                float height = Screen.height;
                var flagx = Define.DesignScreenWidth / width;
                var flagy = Define.DesignScreenHeight / height;
                return flagx > flagy ? flagx : flagy;
            }
        }
        public float WidthPadding { get; private set; }

        #region override

        public void Init()
        {

            var safeArea = Screen.safeArea;
            WidthPadding = safeArea.x;

            Instance = this;
            windows = new Dictionary<string, UIWindow>();
            windowStack = new Dictionary<UILayerNames, LinkedList<string>>();
            InitLayer();
            Messager.Instance.AddListener<int, int>(0, MessageId.OnKeyInput, OnKeyInput);
        }

        public void Destroy()
        {
            Messager.Instance.RemoveListener<int, int>(0, MessageId.OnKeyInput, OnKeyInput);
            Instance = null;
            DestroyLayer();
            OnDestroyAsync().Coroutine();
        }

        private async ETTask OnDestroyAsync()
        {
            await DestroyAllWindow();
            windows.Clear();
            windows = null;
            windowStack.Clear();
            windowStack = null;
            Log.Info("UIManagerComponent Destroy");
        }

        private void OnKeyInput(int key, int state)
        {
            if (key == (int) GameKeyCode.Back && (state & InputManager.KeyDown) != 0)
            {
                var win = GetTopWindow();
                if (win != null && win.View != null && win.View.CanBack)
                {
                    win.View.OnInputKeyBack().Coroutine();
                }
            }
        }

        #endregion

        public void ResetSafeArea()
        {
            var safeArea = Screen.safeArea;
            var temp = safeArea.x;
            SetWidthPadding(temp);
        }

        /// <summary>
        /// 获取UI窗口
        /// </summary>
        /// <param name="uiName"></param>
        /// <param name="active">2打开且loading,1打开，-1关闭,0不做限制</param>
        /// <returns></returns>
        public UIWindow GetWindow(string uiName, int active = 0)
        {
            if (windows.TryGetValue(uiName, out var target))
            {
                if (active == 0 || active * (target.Active ? 1 : -1) > 0)
                {
                    if (active == 2)
                    {
                        return target.LoadingState == UIWindowLoadingState.LoadOver ? target : null;
                    }

                    return target;
                }

                return null;
            }

            return null;
        }

        /// <summary>
        /// 获取最上层window
        /// </summary>
        /// <param name="ignore">忽略的层级</param>
        /// <returns></returns>
        public UIWindow GetTopWindow(params UILayerNames[] ignore)
        {
            using (HashSetComponent<UILayerNames> ignores = HashSetComponent<UILayerNames>.Create())
            {
                for (int i = 0; i < ignore.Length; i++)
                {
                    ignores.Add(ignore[i]);
                }

                for (int i = (byte) UILayerNames.TopLayer; i >= 0; i--)
                {
                    var layer = (UILayerNames) i;
                    if (!ignores.Contains(layer))
                    {
                        var win = GetTopWindow(layer);
                        if (win != null) return win;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// 获取最上层window
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public UIWindow GetTopWindow(UILayerNames layer)
        {
            var wins = windowStack[layer];
            if (wins.Count <= 0) return null;
            for (var node = wins.First; node != null; node = node.Next)
            {
                var name = node.Value;
                var win = GetWindow(name, 1);
                if (win != null)
                    return win;
            }

            return null;

        }

        /// <summary>
        /// 获取UI窗口
        /// </summary>
        /// <param name="active">2打开且loading，1打开，-1关闭,0不做限制</param>
        /// <returns></returns>
        public T GetWindow<T>(int active = 0) where T : UIBaseView
        {
            string uiName = TypeInfo<T>.TypeName;
            if (windows != null && windows.TryGetValue(uiName, out var target))
            {
                if (active == 0 || active * (target.Active ? 1 : -1) > 0)
                {
                    if (active == 2)
                    {
                        return target.LoadingState == UIWindowLoadingState.LoadOver ? target as T : null;
                    }

                    return target.View as T;
                }

                return null;
            }

            return null;
        }

        /// <summary>
        /// 关闭窗体
        /// </summary>
        /// <param name="window"></param>
        public async ETTask CloseWindow(UIBaseView window)
        {
            string uiName = window.GetType().Name;
            await CloseWindow(uiName);
        }

        /// <summary>
        /// 关闭窗体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public async ETTask CloseWindow<T>()
        {
            string uiName = TypeInfo<T>.TypeName;
            await CloseWindow(uiName);
        }

        /// <summary>
        /// 关闭窗体
        /// </summary>
        /// <param name="uiName"></param>
        public async ETTask CloseWindow(string uiName)
        {
            var target = GetWindow(uiName, 1);
            if (target == null) return;
            while (target.LoadingState != UIWindowLoadingState.LoadOver)
            {
                await TimerManager.Instance.WaitAsync(1);
            }

            RemoveFromStack(target);
            InnerCloseWindow(target);
        }

        /// <summary>
        /// 通过层级关闭
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="exceptUINames"></param>
        public async ETTask CloseWindowByLayer(UILayerNames layer, params string[] exceptUINames)
        {
            using (HashSetComponent<string> dictUINames = HashSetComponent<string>.Create())
            {
                if (exceptUINames != null && exceptUINames.Length > 0)
                {
                    for (int i = 0; i < exceptUINames.Length; i++)
                    {
                        dictUINames.Add(exceptUINames[i]);
                    }
                }
                using (ListComponent<ETTask> taskScheduler = ListComponent<ETTask>.Create())
                {
                    foreach (var item in windows)
                    {
                        if (item.Value.Layer == layer && (dictUINames == null || !dictUINames.Contains(item.Key)))
                        {
                            taskScheduler.Add(CloseWindow(item.Key));
                        }
                    }

                    await ETTaskHelper.WaitAll(taskScheduler);
                }
            }
        }

        /// <summary>
        /// 销毁窗体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public async ETTask DestroyWindow<T>(bool clear = false) where T : UIBaseView
        {
            string uiName = TypeInfo<T>.TypeName;
            await DestroyWindow(uiName, clear);
        }

        /// <summary>
        /// 销毁窗体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public async ETTask DestroyWindow(string uiName, bool clear = false)
        {
            var target = GetWindow(uiName);
            if (target != null)
            {
                await CloseWindow(uiName);
                InnerDestroyWindow(target, clear);
                windows.Remove(target.Name);
                target.Dispose();
            }
        }

        /// <summary>
        /// 销毁隐藏状态的窗口
        /// </summary>
        public async ETTask DestroyUnShowWindow()
        {
            using (ListComponent<ETTask> taskScheduler = ListComponent<ETTask>.Create())
            {
                foreach (var key in windows.Keys.ToList())
                {
                    if (!windows[key].Active)
                    {
                        taskScheduler.Add(DestroyWindow(key));
                    }
                }

                await ETTaskHelper.WaitAll(taskScheduler);
            }
        }

        /// <summary>
        /// 打开窗口 对应 <see cref="IOnCreate"/>
        /// </summary>
        /// <param name="fullname">类名</param>
        /// <param name="path">预制体路径</param>
        /// <param name="layerName">UI层级</param>
        /// <returns></returns>
        public async ETTask<UIBaseView> OpenWindow(string fullname, string path,
            UILayerNames layerName = UILayerNames.NormalLayer)
        {
            string uiName = fullname;
            var target = GetWindow(uiName);
            if (target == null)
            {
                target = InitWindow(fullname, path, layerName);
                windows[uiName] = target;
            }

            target.Layer = layerName;
            return await InnerOpenWindow(target);

        }

        /// <summary>
        /// 打开窗口 对应 <see cref="IOnCreate"/>
        /// </summary>
        /// <param name="fullname">类名</param>
        /// <param name="path">预制体路径</param>
        /// <param name="para">参数</param>
        /// <param name="layerName">UI层级</param>
        /// <returns></returns>
        public async ETTask<UIBaseView> OpenWindow(string fullname, string path, object[] para,
            UILayerNames layerName = UILayerNames.NormalLayer)
        {
            string uiName = fullname;
            var target = GetWindow(uiName);
            if (target == null)
            {
                target = InitWindow(fullname, path, layerName);
                windows[uiName] = target;
            }

            target.Layer = layerName;
            return await InnerOpenWindow(target, para);

        }

        /// <summary>
        /// 打开窗口 对应 <see cref="IOnCreate{P1}"/>
        /// </summary>
        /// <param name="fullname">类名</param>
        /// <param name="path">预制体路径</param>
        /// <param name="layerName">UI层级</param>
        /// <returns></returns>
        public async ETTask<UIBaseView> OpenWindow<P1>(string fullname, string path, P1 p1,
            UILayerNames layerName = UILayerNames.NormalLayer)
        {
            string uiName = fullname;
            var target = GetWindow(uiName);
            if (target == null)
            {
                target = InitWindow(fullname, path, layerName);
                windows[uiName] = target;
            }

            target.Layer = layerName;
            return await InnerOpenWindow(target, p1);

        }

        /// <summary>
        /// 打开窗口 对应 <see cref="IOnCreate"/>
        /// </summary>
        /// <param name="path">预制体路径</param>
        /// <param name="layerName">UI层级</param>
        /// <typeparam name="T">要打开的窗口</typeparam>
        /// <returns></returns>
        public async ETTask<T> OpenWindow<T>(string path,
            UILayerNames layerName = UILayerNames.NormalLayer) where T : UIBaseView, IOnCreate
        {
            string uiName = TypeInfo<T>.TypeName;
            var target = GetWindow(uiName);
            if (target == null)
            {
                target = InitWindow<T>(path, layerName);
                windows[uiName] = target;
            }

            target.Layer = layerName;
            return await InnerOpenWindow<T>(target);

        }

        /// <summary>
        /// 打开窗口 对应 <see cref="IOnCreate{P1}"/>
        /// </summary>
        /// <param name="path">预制体路径</param>
        /// <param name="p1"></param>
        /// <param name="layerName">UI层级</param>
        /// <typeparam name="T">要打开的窗口</typeparam>
        /// <typeparam name="P1"></typeparam>
        /// <returns></returns>
        public async ETTask<T> OpenWindow<T, P1>(string path, P1 p1,
            UILayerNames layerName = UILayerNames.NormalLayer)
            where T : UIBaseView, IOnCreate, IOnEnable<P1>
        {

            string uiName = TypeInfo<T>.TypeName;
            var target = GetWindow(uiName);
            if (target == null)
            {
                target = InitWindow<T>(path, layerName);
                windows[uiName] = target;
            }

            target.Layer = layerName;
            return await InnerOpenWindow<T, P1>(target, p1);

        }

        /// <summary>
        /// 打开窗口 对应 <see cref="IOnCreate{P1,P2}"/>
        /// </summary>
        /// <param name="path">预制体路径</param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="layerName">UI层级</param>
        /// <typeparam name="T">要打开的窗口</typeparam>
        /// <typeparam name="P1"></typeparam>
        /// <typeparam name="P2"></typeparam>
        /// <returns></returns>
        public async ETTask<T> OpenWindow<T, P1, P2>(string path, P1 p1, P2 p2,
            UILayerNames layerName = UILayerNames.NormalLayer)
            where T : UIBaseView, IOnCreate, IOnEnable<P1, P2>
        {

            string uiName = TypeInfo<T>.TypeName;
            var target = GetWindow(uiName);
            if (target == null)
            {
                target = InitWindow<T>(path, layerName);
                windows[uiName] = target;
            }

            target.Layer = layerName;
            return await InnerOpenWindow<T, P1, P2>(target, p1, p2);

        }

        /// <summary>
        /// 打开窗口 对应 <see cref="IOnCreate{P1,P2,P3}"/>
        /// </summary>
        /// <param name="path">预制体路径</param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="layerName">UI层级</param>
        /// <typeparam name="T">要打开的窗口</typeparam>
        /// <typeparam name="P1"></typeparam>
        /// <typeparam name="P2"></typeparam>
        /// <typeparam name="P3"></typeparam>
        /// <returns></returns>
        public async ETTask<T> OpenWindow<T, P1, P2, P3>(string path, P1 p1, P2 p2, P3 p3,
            UILayerNames layerName = UILayerNames.NormalLayer)
            where T : UIBaseView, IOnCreate, IOnEnable<P1, P2, P3>
        {

            string uiName = TypeInfo<T>.TypeName;
            var target = GetWindow(uiName);
            if (target == null)
            {
                target = InitWindow<T>(path, layerName);
                windows[uiName] = target;
            }

            target.Layer = layerName;
            return await InnerOpenWindow<T, P1, P2, P3>(target, p1, p2, p3);

        }

        /// <summary>
        /// 打开窗口 对应 <see cref="IOnCreate{P1,P2,P3,P4}"/>
        /// </summary>
        /// <param name="path">预制体路径</param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="p4"></param>
        /// <param name="layerName">UI层级</param>
        /// <typeparam name="T">要打开的窗口</typeparam>
        /// <typeparam name="P1"></typeparam>
        /// <typeparam name="P2"></typeparam>
        /// <typeparam name="P3"></typeparam>
        /// <typeparam name="P4"></typeparam>
        /// <returns></returns>
        public async ETTask<T> OpenWindow<T, P1, P2, P3, P4>(string path, P1 p1, P2 p2, P3 p3, P4 p4,
            UILayerNames layerName = UILayerNames.NormalLayer)
            where T : UIBaseView, IOnCreate, IOnEnable<P1, P2, P3, P4>
        {

            string uiName = TypeInfo<T>.TypeName;
            var target = GetWindow(uiName);
            if (target == null)
            {
                target = InitWindow<T>(path, layerName);
                windows[uiName] = target;
            }

            target.Layer = layerName;
            return await InnerOpenWindow<T, P1, P2, P3, P4>(target, p1, p2, p3, p4);

        }

        /// <summary>
        /// 打开窗口（返回ETTask） 对应 <see cref="IOnCreate"/>
        /// </summary>
        /// <param name="path">预制体路径</param>
        /// <param name="layerName">UI层级</param>
        /// <typeparam name="T">要打开的窗口</typeparam>
        /// <returns></returns>
        public async ETTask OpenWindowTask<T>(string path, UILayerNames layerName = UILayerNames.NormalLayer)
            where T : UIBaseView, IOnCreate, IOnEnable
        {
            await OpenWindow<T>(path, layerName);
        }

        /// <summary>
        /// 打开窗口（返回ETTask）  对应 <see cref="IOnCreate{P1}"/>
        /// </summary>
        /// <param name="path">预制体路径</param>
        /// <param name="p1"></param>
        /// <param name="layerName">UI层级</param>
        /// <typeparam name="T">要打开的窗口</typeparam>
        /// <typeparam name="P1"></typeparam>
        /// <returns></returns>
        public async ETTask OpenWindowTask<T, P1>(string path, P1 p1, UILayerNames layerName = UILayerNames.NormalLayer)
            where T : UIBaseView, IOnCreate, IOnEnable<P1>
        {
            await OpenWindow<T, P1>(path, p1, layerName);
        }

        /// <summary>
        /// 打开窗口（返回ETTask）  对应 <see cref="IOnCreate{P1,P2}"/>
        /// </summary>
        /// <param name="path">预制体路径</param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="layerName">UI层级</param>
        /// <typeparam name="T">要打开的窗口</typeparam>
        /// <typeparam name="P1"></typeparam>
        /// <typeparam name="P2"></typeparam>
        /// <returns></returns>
        public async ETTask OpenWindowTask<T, P1, P2>(string path, P1 p1, P2 p2,
            UILayerNames layerName = UILayerNames.NormalLayer) where T : UIBaseView, IOnCreate, IOnEnable<P1, P2>
        {
            await OpenWindow<T, P1, P2>(path, p1, p2, layerName);
        }

        /// <summary>
        /// 打开窗口（返回ETTask） 对应 <see cref="IOnCreate{P1,P2,P3}"/>
        /// </summary>
        /// <param name="path">预制体路径</param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="layerName">UI层级</param>
        /// <typeparam name="T">要打开的窗口</typeparam>
        /// <typeparam name="P1"></typeparam>
        /// <typeparam name="P2"></typeparam>
        /// <typeparam name="P3"></typeparam>
        /// <returns></returns>
        public async ETTask OpenWindowTask<T, P1, P2, P3>(string path, P1 p1, P2 p2, P3 p3,
            UILayerNames layerName = UILayerNames.NormalLayer) where T : UIBaseView, IOnCreate, IOnEnable<P1, P2, P3>
        {
            await OpenWindow<T, P1, P2, P3>(path, p1, p2, p3, layerName);
        }

        /// <summary>
        /// 打开窗口（返回ETTask）  对应 <see cref="IOnCreate{P1,P2,P3,P4}"/>
        /// </summary>
        /// <param name="path"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="p4"></param>
        /// <param name="layerName"></param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="P1"></typeparam>
        /// <typeparam name="P2"></typeparam>
        /// <typeparam name="P3"></typeparam>
        /// <typeparam name="P4"></typeparam>
        public async ETTask OpenWindowTask<T, P1, P2, P3, P4>(string path, P1 p1, P2 p2, P3 p3, P4 p4,
            UILayerNames layerName = UILayerNames.NormalLayer)
            where T : UIBaseView, IOnCreate, IOnEnable<P1, P2, P3, P4>
        {
            await OpenWindow<T, P1, P2, P3, P4>(path, p1, p2, p3, p4, layerName);
        }

        /// <summary>
        /// 销毁除指定窗口外所有窗口
        /// </summary>
        /// <param name="typeNames">指定窗口</param>
        public async ETTask DestroyWindowExceptNames(string[] typeNames = null)
        {
            using (HashSetComponent<string> dictUINames = HashSetComponent<string>.Create())
            {
                if (typeNames != null)
                {
                    for (int i = 0; i < typeNames.Length; i++)
                    {
                        dictUINames.Add(typeNames[i]);
                    }
                }
                var keys = windows.Keys.ToArray();
                using (ListComponent<ETTask> taskScheduler = ListComponent<ETTask>.Create())
                {
                    for (int i = windows.Count - 1; i >= 0; i--) 
                    {
                        if (!dictUINames.Contains(keys[i]))
                        {
                            taskScheduler.Add(DestroyWindow(keys[i]));
                        }
                    }

                    await ETTaskHelper.WaitAll(taskScheduler);
                }
            }
            
        }

        /// <summary>
        /// 销毁指定层级外层级所有窗口
        /// </summary>
        /// <param name="layer">指定层级</param>
        public async ETTask DestroyWindowExceptLayer(UILayerNames layer)
        {
            var keys = windows.Keys.ToArray();
            using (ListComponent<ETTask> taskScheduler = ListComponent<ETTask>.Create())
            {
                for (int i = windows.Count - 1; i >= 0; i--)
                {
                    if (windows[keys[i]].Layer != layer)
                    {
                        taskScheduler.Add(DestroyWindow(keys[i]));
                    }
                }

                await ETTaskHelper.WaitAll(taskScheduler);
            }
        }

        /// <summary>
        /// 销毁指定层级所有窗口
        /// </summary>
        /// <param name="layer">指定层级</param>
        public async ETTask DestroyWindowByLayer(UILayerNames layer)
        {
            var keys = windows.Keys.ToArray();
            using (ListComponent<ETTask> taskScheduler = ListComponent<ETTask>.Create())
            {
                for (int i = windows.Count - 1; i >= 0; i--)
                {
                    if (windows[keys[i]].Layer == layer)
                    {
                        taskScheduler.Add(DestroyWindow(windows[keys[i]].Name));
                    }
                }

                await ETTaskHelper.WaitAll(taskScheduler);
            }
        }

        /// <summary>
        /// 销毁所有窗体
        /// </summary>
        public async ETTask DestroyAllWindow()
        {
            var keys = windows.Keys.ToArray();
            using (ListComponent<ETTask> taskScheduler = ListComponent<ETTask>.Create())
            {
                for (int i = windows.Count - 1; i >= 0; i--)
                {
                    taskScheduler.Add(DestroyWindow(windows[keys[i]].Name));
                }

                await ETTaskHelper.WaitAll(taskScheduler);
            }
        }

        /// <summary>
        /// 判断窗口是否打开
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool IsActiveWindow<T>() where T : UIBaseView
        {
            string uiName = TypeInfo<T>.TypeName;
            var target = GetWindow(uiName);
            if (target == null)
            {
                return false;
            }

            return target.Active;
        }

        #region 私有方法

        void InnerDestroyWindow(UIWindow target, bool clear = false)
        {
            var view = target.View;
            if (view != null)
            {
                var obj = view.GetGameObject();
                if (obj)
                {
                    if (GameObjectPoolManager.GetInstance() == null)
                        GameObject.Destroy(obj);
                    else
                        GameObjectPoolManager.GetInstance().RecycleGameObject(obj, clear);
                }

                if (view is II18N i18n)
                    I18NManager.Instance?.RemoveI18NEntity(i18n);
                view.BeforeOnDestroy();
                (view as IOnDestroy)?.OnDestroy();
            }
        }

        /// <summary>
        /// 初始化window
        /// </summary>
        UIWindow InitWindow(string name, string path, UILayerNames layerName)
        {
            UIWindow window = UIWindow.Create();
            window.Name = name;
            window.Active = false;
            window.Layer = layerName;
            window.LoadingState = UIWindowLoadingState.NotStart;
            window.PrefabPath = path;
            var fullName = name;
            if (!fullName.Contains("."))
            {
                fullName = GetType().Namespace + "." + fullName;
            }

            window.View = Activator.CreateInstance(GetType().Assembly.GetType(fullName)) as UIBaseView;
            return window;
        }

        /// <summary>
        /// 初始化window
        /// </summary>
        UIWindow InitWindow<T>(string path, UILayerNames layerName) where T : UIBaseView
        {
            UIWindow window = UIWindow.Create();
            var type = TypeInfo<T>.Type;
            window.Name = type.Name;
            window.Active = false;
            window.Layer = layerName;
            window.LoadingState = UIWindowLoadingState.NotStart;
            window.PrefabPath = path;
            window.View = Activator.CreateInstance<T>();
            return window;
        }

        void Deactivate(UIWindow target)
        {
            var view = target.View;
            if (view != null)
                view.SetActive(false);
        }

        #region 内部加载窗体,依次加载prefab、AwakeSystem、InitializationSystem、OnCreateSystem、OnEnableSystem

        void InnerResetWindowLayer(UIWindow window)
        {
            var target = window;
            var view = target.View;
            var uiTrans = view.GetTransform();
            if (uiTrans != null)
            {
                var layer = GetLayer(target.Layer);
                uiTrans.transform.SetParent(layer.RectTransform, false);
            }

            if (view is IOnWidthPaddingChange)
                OnWidthPaddingChange(view);
        }

        async ETTask<UIBaseView> InnerOpenWindow(UIWindow target)
        {
            CoroutineLock coroutineLock = null;
            try
            {
                coroutineLock =
                    await CoroutineLockManager.Instance.Wait(CoroutineLockType.UIManager, target.GetHashCode());
                target.Active = true;
                UIBaseView res = target.View;
                var needLoad = target.LoadingState == UIWindowLoadingState.NotStart;
                target.LoadingState = UIWindowLoadingState.Loading;
                if (needLoad)
                {
                    await InnerOpenWindowGetGameObject(target.PrefabPath, target);
                }

                InnerResetWindowLayer(target);
                await AddWindowToStack(target);
                target.LoadingState = UIWindowLoadingState.LoadOver;
                return res;
            }
            finally
            {
                coroutineLock?.Dispose();
            }

        }

        async ETTask<UIBaseView> InnerOpenWindow<P1>(UIWindow target, P1 p1)
        {
            CoroutineLock coroutineLock = null;
            try
            {
                coroutineLock =
                    await CoroutineLockManager.Instance.Wait(CoroutineLockType.UIManager, target.GetHashCode());
                target.Active = true;
                UIBaseView res = target.View;
                var needLoad = target.LoadingState == UIWindowLoadingState.NotStart;
                target.LoadingState = UIWindowLoadingState.Loading;
                if (needLoad)
                {
                    await InnerOpenWindowGetGameObject(target.PrefabPath, target);
                }

                InnerResetWindowLayer(target);
                await AddWindowToStack(target, p1);
                target.LoadingState = UIWindowLoadingState.LoadOver;
                return res;
            }
            finally
            {
                coroutineLock?.Dispose();
            }

        }

        async ETTask<T> InnerOpenWindow<T>(UIWindow target) where T : UIBaseView
        {
            CoroutineLock coroutineLock = null;
            try
            {
                coroutineLock =
                    await CoroutineLockManager.Instance.Wait(CoroutineLockType.UIManager, target.GetHashCode());
                target.Active = true;
                T res = target.View as T;
                var needLoad = target.LoadingState == UIWindowLoadingState.NotStart;
                target.LoadingState = UIWindowLoadingState.Loading;
                if (needLoad)
                {
                    await InnerOpenWindowGetGameObject(target.PrefabPath, target);
                }

                InnerResetWindowLayer(target);
                await AddWindowToStack(target);
                target.LoadingState = UIWindowLoadingState.LoadOver;
                return res;
            }
            finally
            {
                coroutineLock?.Dispose();
            }

        }

        async ETTask<T> InnerOpenWindow<T, P1>(UIWindow target, P1 p1) where T : UIBaseView
        {
            CoroutineLock coroutineLock = null;
            try
            {
                coroutineLock =
                    await CoroutineLockManager.Instance.Wait(CoroutineLockType.UIManager, target.GetHashCode());
                target.Active = true;
                T res = target.View as T;
                var needLoad = target.LoadingState == UIWindowLoadingState.NotStart;
                target.LoadingState = UIWindowLoadingState.Loading;
                if (needLoad)
                {
                    await InnerOpenWindowGetGameObject(target.PrefabPath, target);
                }

                InnerResetWindowLayer(target);
                await AddWindowToStack(target, p1);
                target.LoadingState = UIWindowLoadingState.LoadOver;
                return res;
            }
            finally
            {
                coroutineLock?.Dispose();
            }
        }

        async ETTask<T> InnerOpenWindow<T, P1, P2>(UIWindow target, P1 p1, P2 p2) where T : UIBaseView
        {
            CoroutineLock coroutineLock = null;
            try
            {
                coroutineLock =
                    await CoroutineLockManager.Instance.Wait(CoroutineLockType.UIManager, target.GetHashCode());
                target.Active = true;
                T res = target.View as T;
                var needLoad = target.LoadingState == UIWindowLoadingState.NotStart;
                target.LoadingState = UIWindowLoadingState.Loading;
                if (needLoad)
                {
                    await InnerOpenWindowGetGameObject(target.PrefabPath, target);
                }

                InnerResetWindowLayer(target);
                await AddWindowToStack(target, p1, p2);
                target.LoadingState = UIWindowLoadingState.LoadOver;
                return res;
            }
            finally
            {
                coroutineLock?.Dispose();
            }
        }

        async ETTask<T> InnerOpenWindow<T, P1, P2, P3>(UIWindow target, P1 p1, P2 p2, P3 p3) where T : UIBaseView
        {
            CoroutineLock coroutineLock = null;
            try
            {
                coroutineLock =
                    await CoroutineLockManager.Instance.Wait(CoroutineLockType.UIManager, target.GetHashCode());
                target.Active = true;
                T res = target.View as T;
                var needLoad = target.LoadingState == UIWindowLoadingState.NotStart;
                target.LoadingState = UIWindowLoadingState.Loading;
                if (needLoad)
                {
                    await InnerOpenWindowGetGameObject(target.PrefabPath, target);
                }

                InnerResetWindowLayer(target);
                await AddWindowToStack(target, p1, p2, p3);
                target.LoadingState = UIWindowLoadingState.LoadOver;
                return res;
            }
            finally
            {
                coroutineLock?.Dispose();
            }
        }

        async ETTask<T> InnerOpenWindow<T, P1, P2, P3, P4>(UIWindow target, P1 p1, P2 p2, P3 p3, P4 p4)
            where T : UIBaseView
        {
            CoroutineLock coroutineLock = null;
            try
            {
                coroutineLock =
                    await CoroutineLockManager.Instance.Wait(CoroutineLockType.UIManager, target.GetHashCode());
                target.Active = true;
                T res = target.View as T;
                var needLoad = target.LoadingState == UIWindowLoadingState.NotStart;
                target.LoadingState = UIWindowLoadingState.Loading;
                if (needLoad)
                {
                    await InnerOpenWindowGetGameObject(target.PrefabPath, target);
                }

                InnerResetWindowLayer(target);
                await AddWindowToStack(target, p1, p2, p3, p4);
                target.LoadingState = UIWindowLoadingState.LoadOver;
                return res;
            }
            finally
            {
                coroutineLock?.Dispose();
            }
        }

        async ETTask InnerOpenWindowGetGameObject(string path, UIWindow target)
        {
            var view = target.View;

            // await UIWatcherComponent.Instance.OnViewInitializationSystem(view);

            var go = await GameObjectPoolManager.GetInstance().GetGameObjectAsync(path);
            if (go == null)
            {
                Log.Error($"UIManager InnerOpenWindow {target.PrefabPath} fail");
                return;
            }

            var trans = go.transform;
            trans.SetParent(GetLayer(target.Layer).RectTransform, false);
            trans.name = target.Name;

            view.SetTransform(trans);
            (view as IOnCreate)?.OnCreate();
            if (view is II18N i18n)
                I18NManager.Instance.RegisterI18NEntity(i18n);
        }

        #endregion

        /// <summary>
        /// 内部关闭窗体，OnDisableSystem
        /// </summary>
        /// <param name="target"></param>
        void InnerCloseWindow(UIWindow target)
        {
            if (target.Active)
            {
                Deactivate(target);
                target.Active = false;
            }
        }

        /// <summary>
        /// 将窗口移到当前层级最上方
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void MoveWindowToTop<T>() where T : UIBaseView
        {
            string uiName = TypeInfo<T>.TypeName;
            var target = GetWindow(uiName, 1);
            if (target == null)
            {
                return;
            }

            var layerName = target.Layer;
            if (windowStack[layerName].Contains(uiName))
            {
                windowStack[layerName].Remove(uiName);
            }

            windowStack[layerName].AddFirst(uiName);
            InnerAddWindowToStack(target);
        }

        async ETTask AddWindowToStack(UIWindow target)
        {
            var uiName = target.Name;
            var layerName = target.Layer;
            bool isFirst = true;
            if (windowStack[layerName].Contains(uiName))
            {
                isFirst = false;
                windowStack[layerName].Remove(uiName);
            }

            windowStack[layerName].AddFirst(uiName);
            InnerAddWindowToStack(target);
            var view = target.View;
            view.SetActive(true);
            if (isFirst && (layerName == UILayerNames.BackgroundLayer || layerName == UILayerNames.GameBackgroundLayer))
            {
                //如果是背景layer，则销毁所有的normal层|BackgroudLayer
                await CloseWindowByLayer(UILayerNames.NormalLayer);
                await CloseWindowByLayer(UILayerNames.GameLayer);
                await CloseWindowByLayer(UILayerNames.BackgroundLayer, uiName);
                await CloseWindowByLayer(UILayerNames.GameBackgroundLayer, uiName);
            }
        }

        async ETTask AddWindowToStack<P1>(UIWindow target, P1 p1)
        {
            var uiName = target.Name;
            var layerName = target.Layer;
            bool isFirst = true;
            if (windowStack[layerName].Contains(uiName))
            {
                isFirst = false;
                windowStack[layerName].Remove(uiName);
            }

            windowStack[layerName].AddFirst(uiName);
            InnerAddWindowToStack(target);
            var view = target.View;
            view.SetActive(true, p1);
            if (isFirst && (layerName == UILayerNames.BackgroundLayer || layerName == UILayerNames.GameBackgroundLayer))
            {
                //如果是背景layer，则销毁所有的normal层|BackgroudLayer
                await CloseWindowByLayer(UILayerNames.NormalLayer);
                await CloseWindowByLayer(UILayerNames.GameLayer);
                await CloseWindowByLayer(UILayerNames.BackgroundLayer, uiName);
                await CloseWindowByLayer(UILayerNames.GameBackgroundLayer, uiName);
            }
        }

        async ETTask AddWindowToStack<P1, P2>(UIWindow target, P1 p1, P2 p2)
        {
            var uiName = target.Name;
            var layerName = target.Layer;
            bool isFirst = true;
            if (windowStack[layerName].Contains(uiName))
            {
                isFirst = false;
                windowStack[layerName].Remove(uiName);
            }

            windowStack[layerName].AddFirst(uiName);
            InnerAddWindowToStack(target);
            var view = target.View;
            view.SetActive(true, p1, p2);
            if (isFirst && (layerName == UILayerNames.BackgroundLayer || layerName == UILayerNames.GameBackgroundLayer))
            {
                //如果是背景layer，则销毁所有的normal层|BackgroudLayer
                await CloseWindowByLayer(UILayerNames.NormalLayer);
                await CloseWindowByLayer(UILayerNames.GameLayer);
                await CloseWindowByLayer(UILayerNames.BackgroundLayer, uiName);
                await CloseWindowByLayer(UILayerNames.GameBackgroundLayer, uiName);
            }
        }

        async ETTask AddWindowToStack<P1, P2, P3>(UIWindow target, P1 p1, P2 p2, P3 p3)
        {
            var uiName = target.Name;
            var layerName = target.Layer;
            bool isFirst = true;
            if (windowStack[layerName].Contains(uiName))
            {
                isFirst = false;
                windowStack[layerName].Remove(uiName);
            }

            windowStack[layerName].AddFirst(uiName);
            InnerAddWindowToStack(target);
            var view = target.View;
            view.SetActive(true, p1, p2, p3);
            if (isFirst && (layerName == UILayerNames.BackgroundLayer || layerName == UILayerNames.GameBackgroundLayer))
            {
                //如果是背景layer，则销毁所有的normal层|BackgroudLayer
                await CloseWindowByLayer(UILayerNames.NormalLayer);
                await CloseWindowByLayer(UILayerNames.GameLayer);
                await CloseWindowByLayer(UILayerNames.BackgroundLayer, uiName);
                await CloseWindowByLayer(UILayerNames.GameBackgroundLayer, uiName);
            }
        }

        async ETTask AddWindowToStack<P1, P2, P3, P4>(UIWindow target, P1 p1, P2 p2, P3 p3, P4 p4)
        {
            var uiName = target.Name;
            var layerName = target.Layer;
            bool isFirst = true;
            if (windowStack[layerName].Contains(uiName))
            {
                isFirst = false;
                windowStack[layerName].Remove(uiName);
            }

            windowStack[layerName].AddFirst(uiName);
            InnerAddWindowToStack(target);
            var view = target.View;
            view.SetActive(true, p1, p2, p3, p4);
            if (isFirst && (layerName == UILayerNames.BackgroundLayer || layerName == UILayerNames.GameBackgroundLayer))
            {
                //如果是背景layer，则销毁所有的normal层或BackgroundLayer
                await CloseWindowByLayer(UILayerNames.NormalLayer);
                await CloseWindowByLayer(UILayerNames.GameLayer);
                await CloseWindowByLayer(UILayerNames.BackgroundLayer, uiName);
                await CloseWindowByLayer(UILayerNames.GameBackgroundLayer, uiName);
            }
        }

        void InnerAddWindowToStack(UIWindow target)
        {
            var view = target.View;
            var uiTrans = view.GetTransform();
            if (uiTrans != null)
            {
                uiTrans.SetAsLastSibling();
                // GuidanceManager.Instance?.NoticeEvent("Open_"+target.Name);
            }
        }

        /// <summary>
        /// 移除
        /// </summary>
        /// <param name="target"></param>
        void RemoveFromStack(UIWindow target)
        {
            var uiName = target.Name;
            var layerName = target.Layer;
            if (windowStack.ContainsKey(layerName))
            {
                windowStack[layerName].Remove(uiName);
            }
            else
            {
                Log.Error("not layer, name :" + layerName);
            }
        }

        #endregion

        #region 屏幕适配

        /// <summary>
        /// 修改边缘宽度
        /// </summary>
        /// <param name="value"></param>
        public void SetWidthPadding(float value)
        {
            WidthPadding = value;
            foreach (var layer in windowStack.Values)
            {
                if (layer != null)
                {
                    for (LinkedListNode<string> node = layer.First; null != node; node = node.Next)
                    {
                        var target = GetWindow(node.Value);
                        if (target.View is IOnWidthPaddingChange)
                        {
                            OnWidthPaddingChange(target.View);
                        }
                    }
                }
            }
        }

        private void OnWidthPaddingChange(UIBaseView target)
        {
            var rectTrans = target.GetTransform().GetComponent<RectTransform>();
            var padding = WidthPadding;
            
            var safeArea = Screen.safeArea;
            var height = Screen.height;
            var width = Screen.width;
            
            float top = (float) safeArea.top * ScreenSizeFlag;
            float bottom = (height - (float)safeArea.bottom) * ScreenSizeFlag;
#if UNITY_WEBGL
            //竖屏特殊适配
            if (width < height)
            {
                if (top == 0 && bottom == height)
                {
                    float flag = Define.DesignScreenWidth / width;
                    var dHeight = flag * height;
                    if (Define.DesignScreenHeight < dHeight)
                    {
                        top = bottom = (dHeight - Define.DesignScreenHeight) / 2;
                    }
                }
            }
#endif
            rectTrans.offsetMin = new Vector2(padding * (1 - rectTrans.anchorMin.x), bottom * rectTrans.anchorMax.y);
            rectTrans.offsetMax = new Vector2(-padding * rectTrans.anchorMax.x, -top * (1 - rectTrans.anchorMin.y));
        }

        private readonly Vector2 hidePos = new Vector2(9999, 9999);
        public Vector2 ScreenPointToUILocalPoint(RectTransform parentRT, Vector2 screenPoint)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRT, screenPoint, UICamera,
                    out Vector2 localPos))
            {
                return localPos;
            }
            return hidePos;
        }
        #endregion

        #region Layer

        public Camera GetUICamera()
        {
            return UICamera;
        }

        public UIBaseView GetView(string uiName)
        {
            var res = GetWindow(uiName);
            if (res != null)
            {
                return res.View;
            }

            return null;
        }

        public UILayer GetLayer(UILayerNames layer)
        {
            if (layers.TryGetValue(layer, out var res))
            {
                return res;
            }

            return null;
        }

        #endregion
    }
}