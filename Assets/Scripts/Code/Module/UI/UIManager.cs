using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// fd: UI管理类，所有UI都应该通过该管理类进行创建 
    /// UIManager.Instance.OpenWindow<T>();
    /// 提供UI操作、UI层级、UI消息、UI资源加载、UI调度、UI缓存等管理
    /// </summary>
    public partial class UIManager : IManager
    {

        public static UIManager Instance { get; private set; }

        private Dictionary<string, UIWindow> windows; //所有存活的窗体  {uiName:window}
        private Dictionary<UILayerNames, LinkedList<string>> windowStack; //窗口记录队列
        public int MaxOderPerWindow { get; private set; } = 10;
        public float ScreenSizeFlag { get; private set; }
        public float WidthPadding { get; private set; }

        #region override

        public void Init()
        {
            Instance = this;
            this.windows = new Dictionary<string, UIWindow>();
            this.windowStack = new Dictionary<UILayerNames, LinkedList<string>>();
            InitLayer();
        }

        public void Destroy()
        {
            Instance = null;
            DestroyLayer();
            OnDestroyAsync().Coroutine();
        }

        private async ETTask OnDestroyAsync()
        {
            await this.DestroyAllWindow();
            this.windows.Clear();
            this.windows = null;
            this.windowStack.Clear();
            this.windowStack = null;
            // InputWatcherComponent.Instance?.RemoveInputUIBaseView(this);
            Log.Info("UIManagerComponent Dispose");
        }

        #endregion


        /// <summary>
        /// 获取UI窗口
        /// </summary>
        /// <param name="uiName"></param>
        /// <param name="active">1打开，-1关闭,0不做限制</param>
        /// <returns></returns>
        public UIWindow GetWindow(string uiName, int active = 0)
        {
            if (this.windows.TryGetValue(uiName, out var target))
            {
                if (active == 0 || active == (target.Active ? 1 : -1))
                {
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
                        var win = this.GetTopWindow(layer);
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
            var wins = this.windowStack[layer];
            if (wins.Count <= 0) return null;
            for (var node = wins.First; node != null; node = node.Next)
            {
                var name = node.Value;
                var win = this.GetWindow(name, 1);
                if (win != null)
                    return win;
            }

            return null;

        }

        /// <summary>
        /// 获取UI窗口
        /// </summary>
        /// <param name="active">1打开，-1关闭,0不做限制</param>
        /// <returns></returns>
        public T GetWindow<T>(int active = 0) where T : UIBaseView
        {
            string uiName = TypeInfo<T>.TypeName;
            if (this != null && this.windows != null && this.windows.TryGetValue(uiName, out var target))
            {
                if (active == 0 || active == (target.Active ? 1 : -1))
                {
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
            await this.CloseWindow(uiName);
        }

        /// <summary>
        /// 关闭窗体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public async ETTask CloseWindow<T>()
        {
            string uiName = TypeInfo<T>.TypeName;
            await this.CloseWindow(uiName);
        }

        /// <summary>
        /// 关闭窗体
        /// </summary>
        /// <param name="uiName"></param>
        public async ETTask CloseWindow(string uiName)
        {
            var target = this.GetWindow(uiName, 1);
            if (target == null) return;
            while (target.LoadingState != UIWindowLoadingState.LoadOver)
            {
                await TimerManager.Instance.WaitAsync(1);
            }

            this.RemoveFromStack(target);
            this.InnnerCloseWindow(target);
        }

        /// <summary>
        /// 通过层级关闭
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="exceptUINames"></param>
        public async ETTask CloseWindowByLayer(UILayerNames layer, params string[] exceptUINames)
        {
            Dictionary<string, bool> dictUINames = null;
            if (exceptUINames != null && exceptUINames.Length > 0)
            {
                dictUINames = new Dictionary<string, bool>();
                for (int i = 0; i < exceptUINames.Length; i++)
                {
                    dictUINames[exceptUINames[i]] = true;
                }
            }

            using (ListComponent<ETTask> taskScheduler = ListComponent<ETTask>.Create())
            {
                foreach (var item in this.windows)
                {
                    if (item.Value.Layer == layer && (dictUINames == null || !dictUINames.ContainsKey(item.Key)))
                    {
                        taskScheduler.Add(this.CloseWindow(item.Key));
                    }
                }

                await ETTaskHelper.WaitAll(taskScheduler);
            }
        }

        /// <summary>
        /// 销毁窗体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public async ETTask DestroyWindow<T>(bool clear = false) where T : UIBaseView
        {
            string uiName = TypeInfo<T>.TypeName;
            await this.DestroyWindow(uiName,clear);
        }

        /// <summary>
        /// 销毁窗体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public async ETTask DestroyWindow(string uiName,bool clear = false)
        {
            var target = this.GetWindow(uiName);
            if (target != null)
            {
                await this.CloseWindow(uiName);
                InnerDestroyWindow(target,clear);
                this.windows.Remove(target.Name);
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
                foreach (var key in this.windows.Keys.ToList())
                {
                    if (!this.windows[key].Active)
                    {
                        taskScheduler.Add(this.DestroyWindow(key));
                    }
                }

                await ETTaskHelper.WaitAll(taskScheduler);
            }
        }

        /// <summary>
        /// 打开窗口 对应 <see cref="IOnCreate"/>
        /// </summary>
        /// <param name="path">预制体路径</param>
        /// <param name="layerName">UI层级</param>
        /// <param name="banKey">是否禁止监听返回键事件</param>
        /// <typeparam name="T">要打开的窗口</typeparam>
        /// <returns></returns>
        public async ETTask<T> OpenWindow<T>(string path,
            UILayerNames layerName = UILayerNames.NormalLayer, bool banKey = true) where T : UIBaseView, IOnCreate
        {
            string uiName = TypeInfo<T>.TypeName;
            var target = this.GetWindow(uiName);
            if (target == null)
            {
                target = this.InitWindow<T>(path, layerName);
                this.windows[uiName] = target;
            }

            target.Layer = layerName;
            target.BanKey = banKey;
            return await this.InnerOpenWindow<T>(target);

        }

        /// <summary>
        /// 打开窗口 对应 <see cref="IOnCreate{P1}"/>
        /// </summary>
        /// <param name="path">预制体路径</param>
        /// <param name="layerName">UI层级</param>
        /// <param name="banKey">是否禁止监听返回键事件</param>
        /// <typeparam name="T">要打开的窗口</typeparam>
        /// <returns></returns>
        public async ETTask<T> OpenWindow<T, P1>(string path, P1 p1,
            UILayerNames layerName = UILayerNames.NormalLayer, bool banKey = true)
            where T : UIBaseView, IOnCreate, IOnEnable<P1>
        {

            string uiName = TypeInfo<T>.TypeName;
            var target = this.GetWindow(uiName);
            if (target == null)
            {
                target = this.InitWindow<T>(path, layerName);
                this.windows[uiName] = target;
            }

            target.Layer = layerName;
            target.BanKey = banKey;
            return await this.InnerOpenWindow<T, P1>(target, p1);

        }

        /// <summary>
        /// 打开窗口 对应 <see cref="IOnCreate{P1,P2}"/>
        /// </summary>
        /// <param name="path">预制体路径</param>
        /// <param name="layerName">UI层级</param>
        /// <param name="banKey">是否禁止监听返回键事件</param>
        /// <typeparam name="T">要打开的窗口</typeparam>
        /// <returns></returns>
        public async ETTask<T> OpenWindow<T, P1, P2>(string path, P1 p1, P2 p2,
            UILayerNames layerName = UILayerNames.NormalLayer, bool banKey = true)
            where T : UIBaseView, IOnCreate, IOnEnable<P1, P2>
        {

            string uiName = TypeInfo<T>.TypeName;
            var target = this.GetWindow(uiName);
            if (target == null)
            {
                target = this.InitWindow<T>(path, layerName);
                this.windows[uiName] = target;
            }

            target.Layer = layerName;
            target.BanKey = banKey;
            return await this.InnerOpenWindow<T, P1, P2>(target, p1, p2);

        }

        /// <summary>
        /// 打开窗口 对应 <see cref="IOnCreate{P1,P2,P3}"/>
        /// </summary>
        /// <param name="path">预制体路径</param>
        /// <param name="layerName">UI层级</param>
        /// <param name="banKey">是否禁止监听返回键事件</param>
        /// <typeparam name="T">要打开的窗口</typeparam>
        /// <returns></returns>
        public async ETTask<T> OpenWindow<T, P1, P2, P3>(string path, P1 p1, P2 p2, P3 p3,
            UILayerNames layerName = UILayerNames.NormalLayer, bool banKey = true)
            where T : UIBaseView, IOnCreate, IOnEnable<P1, P2, P3>
        {

            string uiName = TypeInfo<T>.TypeName;
            var target = this.GetWindow(uiName);
            if (target == null)
            {
                target = this.InitWindow<T>(path, layerName);
                this.windows[uiName] = target;
            }

            target.Layer = layerName;
            target.BanKey = banKey;
            return await this.InnerOpenWindow<T, P1, P2, P3>(target, p1, p2, p3);

        }

        /// <summary>
        /// 打开窗口 对应 <see cref="IOnCreate{P1,P2,P3,P4}"/>
        /// </summary>
        /// <param name="path">预制体路径</param>
        /// <param name="layerName">UI层级</param>
        /// <param name="banKey">是否禁止监听返回键事件</param>
        /// <typeparam name="T">要打开的窗口</typeparam>
        /// <returns></returns>
        public async ETTask<T> OpenWindow<T, P1, P2, P3, P4>(string path, P1 p1, P2 p2, P3 p3, P4 p4,
            UILayerNames layerName = UILayerNames.NormalLayer, bool banKey = true)
            where T : UIBaseView, IOnCreate, IOnEnable<P1, P2, P3, P4>
        {

            string uiName = TypeInfo<T>.TypeName;
            var target = this.GetWindow(uiName);
            if (target == null)
            {
                target = this.InitWindow<T>(path, layerName);
                this.windows[uiName] = target;
            }

            target.Layer = layerName;
            target.BanKey = banKey;
            return await this.InnerOpenWindow<T, P1, P2, P3, P4>(target, p1, p2, p3, p4);

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
            await this.OpenWindow<T>(path, layerName);
        }

        /// <summary>
        /// 打开窗口（返回ETTask）  对应 <see cref="IOnCreate{P1}"/>
        /// </summary>
        /// <param name="path">预制体路径</param>
        /// <param name="layerName">UI层级</param>
        /// <typeparam name="T">要打开的窗口</typeparam>
        /// <returns></returns>
        public async ETTask OpenWindowTask<T, P1>(string path, P1 p1, UILayerNames layerName = UILayerNames.NormalLayer)
            where T : UIBaseView, IOnCreate, IOnEnable<P1>
        {
            await this.OpenWindow<T, P1>(path, p1, layerName);
        }

        /// <summary>
        /// 打开窗口（返回ETTask）  对应 <see cref="IOnCreate{P1,P2}"/>
        /// </summary>
        /// <param name="path">预制体路径</param>
        /// <param name="layerName">UI层级</param>
        /// <typeparam name="T">要打开的窗口</typeparam>
        /// <returns></returns>
        public async ETTask OpenWindowTask<T, P1, P2>(string path, P1 p1, P2 p2,
            UILayerNames layerName = UILayerNames.NormalLayer) where T : UIBaseView, IOnCreate, IOnEnable<P1, P2>
        {
            await this.OpenWindow<T, P1, P2>(path, p1, p2, layerName);
        }

        /// <summary>
        /// 打开窗口（返回ETTask） 对应 <see cref="IOnCreate{P1,P2,P3}"/>
        /// </summary>
        /// <param name="path">预制体路径</param>
        /// <param name="layerName">UI层级</param>
        /// <typeparam name="T">要打开的窗口</typeparam>
        /// <returns></returns>
        public async ETTask OpenWindowTask<T, P1, P2, P3>(string path, P1 p1, P2 p2, P3 p3,
            UILayerNames layerName = UILayerNames.NormalLayer) where T : UIBaseView, IOnCreate, IOnEnable<P1, P2, P3>
        {
            await this.OpenWindow<T, P1, P2, P3>(path, p1, p2, p3, layerName);
        }

        /// <summary>
        /// 打开窗口（返回ETTask）  对应 <see cref="IOnCreate{P1,P2,P3,P4}"/>
        /// </summary>
        /// <param name="path">预制体路径</param>
        /// <param name="layerName">UI层级</param>
        /// <typeparam name="T">要打开的窗口</typeparam>
        /// <returns></returns>
        public async ETTask OpenWindowTask<T, P1, P2, P3, P4>(string path, P1 p1, P2 p2, P3 p3, P4 p4,
            UILayerNames layerName = UILayerNames.NormalLayer)
            where T : UIBaseView, IOnCreate, IOnEnable<P1, P2, P3, P4>
        {
            await this.OpenWindow<T, P1, P2, P3, P4>(path, p1, p2, p3, p4, layerName);
        }

        /// <summary>
        /// 销毁除指定窗口外所有窗口
        /// </summary>
        /// <param name="typeNames">指定窗口</param>
        public async ETTask DestroyWindowExceptNames(string[] typeNames = null)
        {
            Dictionary<string, bool> dictUINames = new Dictionary<string, bool>();
            if (typeNames != null)
            {
                for (int i = 0; i < typeNames.Length; i++)
                {
                    dictUINames[typeNames[i]] = true;
                }
            }

            var keys = this.windows.Keys.ToArray();
            using (ListComponent<ETTask> taskScheduler = ListComponent<ETTask>.Create())
            {
                for (int i = this.windows.Count - 1; i >= 0; i--)
                {
                    if (!dictUINames.ContainsKey(keys[i]))
                    {
                        taskScheduler.Add(this.DestroyWindow(keys[i]));
                    }
                }

                await ETTaskHelper.WaitAll(taskScheduler);
            }
        }

        /// <summary>
        /// 销毁指定层级外层级所有窗口
        /// </summary>
        /// <param name="layer">指定层级</param>
        public async ETTask DestroyWindowExceptLayer(UILayerNames layer)
        {
            var keys = this.windows.Keys.ToArray();
            using (ListComponent<ETTask> taskScheduler = ListComponent<ETTask>.Create())
            {
                for (int i = this.windows.Count - 1; i >= 0; i--)
                {
                    if (this.windows[keys[i]].Layer != layer)
                    {
                        taskScheduler.Add(this.DestroyWindow(keys[i]));
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
            var keys = this.windows.Keys.ToArray();
            using (ListComponent<ETTask> taskScheduler = ListComponent<ETTask>.Create())
            {
                for (int i = this.windows.Count - 1; i >= 0; i--)
                {
                    if (this.windows[keys[i]].Layer == layer)
                    {
                        taskScheduler.Add(this.DestroyWindow(this.windows[keys[i]].Name));
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
            var keys = this.windows.Keys.ToArray();
            using (ListComponent<ETTask> taskScheduler = ListComponent<ETTask>.Create())
            {
                for (int i = this.windows.Count - 1; i >= 0; i--)
                {
                    taskScheduler.Add(this.DestroyWindow(this.windows[keys[i]].Name));
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
            var target = this.GetWindow(uiName);
            if (target == null)
            {
                return false;
            }

            return target.Active;
        }

        #region 私有方法

        void InnerDestroyWindow(UIWindow target,bool clear = false)
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
                        GameObjectPoolManager.GetInstance().RecycleGameObject(obj,clear);
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
                await this.AddWindowToStack(target);
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
                await this.AddWindowToStack(target, p1);
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
                await this.AddWindowToStack(target, p1, p2);
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
                await this.AddWindowToStack(target, p1, p2, p3);
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
                await this.AddWindowToStack(target, p1, p2, p3, p4);
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
                Log.Error(string.Format("UIManager InnerOpenWindow {0} faild", target.PrefabPath));
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
        void InnnerCloseWindow(UIWindow target)
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
            var target = this.GetWindow(uiName, 1);
            if (target == null)
            {
                return;
            }

            var layerName = target.Layer;
            if (this.windowStack[layerName].Contains(uiName))
            {
                this.windowStack[layerName].Remove(uiName);
            }

            this.windowStack[layerName].AddFirst(uiName);
            InnerAddWindowToStack(target);
        }

        async ETTask AddWindowToStack(UIWindow target)
        {
            var uiName = target.Name;
            var layerName = target.Layer;
            bool isFirst = true;
            if (this.windowStack[layerName].Contains(uiName))
            {
                isFirst = false;
                this.windowStack[layerName].Remove(uiName);
            }

            this.windowStack[layerName].AddFirst(uiName);
            InnerAddWindowToStack(target);
            var view = target.View;
            view.SetActive(true);
            if (isFirst && (layerName == UILayerNames.BackgroudLayer || layerName == UILayerNames.GameBackgroudLayer))
            {
                //如果是背景layer，则销毁所有的normal层|BackgroudLayer
                await this.CloseWindowByLayer(UILayerNames.NormalLayer);
                await this.CloseWindowByLayer(UILayerNames.GameLayer);
                await this.CloseWindowByLayer(UILayerNames.BackgroudLayer, uiName);
                await this.CloseWindowByLayer(UILayerNames.GameBackgroudLayer, uiName);
            }
        }

        async ETTask AddWindowToStack<P1>(UIWindow target, P1 p1)
        {
            var uiName = target.Name;
            var layerName = target.Layer;
            bool isFirst = true;
            if (this.windowStack[layerName].Contains(uiName))
            {
                isFirst = false;
                this.windowStack[layerName].Remove(uiName);
            }

            this.windowStack[layerName].AddFirst(uiName);
            InnerAddWindowToStack(target);
            var view = target.View;
            view.SetActive(true, p1);
            if (isFirst && (layerName == UILayerNames.BackgroudLayer || layerName == UILayerNames.GameBackgroudLayer))
            {
                //如果是背景layer，则销毁所有的normal层|BackgroudLayer
                await this.CloseWindowByLayer(UILayerNames.NormalLayer);
                await this.CloseWindowByLayer(UILayerNames.GameLayer);
                await this.CloseWindowByLayer(UILayerNames.BackgroudLayer, uiName);
                await this.CloseWindowByLayer(UILayerNames.GameBackgroudLayer, uiName);
            }
        }

        async ETTask AddWindowToStack<P1, P2>(UIWindow target, P1 p1, P2 p2)
        {
            var uiName = target.Name;
            var layerName = target.Layer;
            bool isFirst = true;
            if (this.windowStack[layerName].Contains(uiName))
            {
                isFirst = false;
                this.windowStack[layerName].Remove(uiName);
            }

            this.windowStack[layerName].AddFirst(uiName);
            InnerAddWindowToStack(target);
            var view = target.View;
            view.SetActive(true, p1, p2);
            if (isFirst && (layerName == UILayerNames.BackgroudLayer || layerName == UILayerNames.GameBackgroudLayer))
            {
                //如果是背景layer，则销毁所有的normal层|BackgroudLayer
                await this.CloseWindowByLayer(UILayerNames.NormalLayer);
                await this.CloseWindowByLayer(UILayerNames.GameLayer);
                await this.CloseWindowByLayer(UILayerNames.BackgroudLayer, uiName);
                await this.CloseWindowByLayer(UILayerNames.GameBackgroudLayer, uiName);
            }
        }

        async ETTask AddWindowToStack<P1, P2, P3>(UIWindow target, P1 p1, P2 p2, P3 p3)
        {
            var uiName = target.Name;
            var layerName = target.Layer;
            bool isFirst = true;
            if (this.windowStack[layerName].Contains(uiName))
            {
                isFirst = false;
                this.windowStack[layerName].Remove(uiName);
            }

            this.windowStack[layerName].AddFirst(uiName);
            InnerAddWindowToStack(target);
            var view = target.View;
            view.SetActive(true, p1, p2, p3);
            if (isFirst && (layerName == UILayerNames.BackgroudLayer || layerName == UILayerNames.GameBackgroudLayer))
            {
                //如果是背景layer，则销毁所有的normal层|BackgroudLayer
                await this.CloseWindowByLayer(UILayerNames.NormalLayer);
                await this.CloseWindowByLayer(UILayerNames.GameLayer);
                await this.CloseWindowByLayer(UILayerNames.BackgroudLayer, uiName);
                await this.CloseWindowByLayer(UILayerNames.GameBackgroudLayer, uiName);
            }
        }

        async ETTask AddWindowToStack<P1, P2, P3, P4>(UIWindow target, P1 p1, P2 p2, P3 p3, P4 p4)
        {
            var uiName = target.Name;
            var layerName = target.Layer;
            bool isFirst = true;
            if (this.windowStack[layerName].Contains(uiName))
            {
                isFirst = false;
                this.windowStack[layerName].Remove(uiName);
            }

            this.windowStack[layerName].AddFirst(uiName);
            InnerAddWindowToStack(target);
            var view = target.View;
            view.SetActive(true, p1, p2, p3, p4);
            if (isFirst && (layerName == UILayerNames.BackgroudLayer || layerName == UILayerNames.GameBackgroudLayer))
            {
                //如果是背景layer，则销毁所有的normal层|BackgroudLayer
                await this.CloseWindowByLayer(UILayerNames.NormalLayer);
                await this.CloseWindowByLayer(UILayerNames.GameLayer);
                await this.CloseWindowByLayer(UILayerNames.BackgroudLayer, uiName);
                await this.CloseWindowByLayer(UILayerNames.GameBackgroudLayer, uiName);
            }
        }

        void InnerAddWindowToStack(UIWindow target)
        {
            var view = target.View;
            var uiTrans = view.GetTransform();
            if (uiTrans != null)
            {
                uiTrans.SetAsLastSibling();
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
            if (this.windowStack.ContainsKey(layerName))
            {
                this.windowStack[layerName].Remove(uiName);
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
            this.WidthPadding = value;
            foreach (var layer in this.windowStack.Values)
            {
                if (layer != null)
                {
                    for (LinkedListNode<string> node = layer.First; null != node; node = node.Next)
                    {
                        var target = this.GetWindow(node.Value);
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
            var pandding = WidthPadding;
            rectTrans.offsetMin = new Vector2(pandding * (1 - rectTrans.anchorMin.x), 0);
            rectTrans.offsetMax = new Vector2(-pandding * rectTrans.anchorMax.x, 0);
        }

        #endregion

        #region Layer

        public Camera GetUICamera()
        {
            return UICamera;
        }

        public UIBaseView GetView(string uiName)
        {
            var res = this.GetWindow(uiName);
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