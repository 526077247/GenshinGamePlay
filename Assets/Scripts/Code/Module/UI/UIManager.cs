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
    public class UIManager:IManager
    {

        public static UIManager Instance { get; private set; }
        
        public Dictionary<string, UIWindow> windows;//所有存活的窗体  {ui_name:window}
        public Dictionary<UILayerNames, LinkedList<string>> window_stack;//窗口记录队列
        public int MaxOderPerWindow = 10;
        public float ScreenSizeflag { get; set; }
        public float WidthPadding;
        
        #region override

        public void Init()
        {
            Instance = this;
            this.windows = new Dictionary<string, UIWindow>();
            this.window_stack = new Dictionary<UILayerNames, LinkedList<string>>();
            
        }

        public void Destroy()
        {
            Instance = null;
            OnDestroyAsync().Coroutine();
        }

        private async ETTask OnDestroyAsync()
        {
            await this.DestroyAllWindow();
            this.windows.Clear();
            this.windows = null;
            this.window_stack.Clear();
            this.window_stack = null;
            // InputWatcherComponent.Instance?.RemoveInputUIBaseView(this);
            Log.Info("UIManagerComponent Dispose");
        }

        #endregion
        
        
        /// <summary>
        /// 获取UI窗口
        /// </summary>
        /// <param name="ui_name"></param>
        /// <param name="active">1打开，-1关闭,0不做限制</param>
        /// <returns></returns>
        public UIWindow GetWindow( string ui_name, int active = 0)
        {
            if (this.windows.TryGetValue(ui_name, out var target))
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
                for (int i = (byte)UILayerNames.TopLayer; i >=0; i--)
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
            var wins = this.window_stack[layer];
            if (wins.Count <= 0) return null;
            for (var node = wins.First; node!=null; node=node.Next)
            {
                var name = node.Value;
                var win = this.GetWindow(name,1);
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
        public T GetWindow<T>( int active = 0) where T : UIBaseView
        {
            string ui_name = typeof(T).Name;
            if (this!=null&&this.windows!=null&&this.windows.TryGetValue(ui_name, out var target))
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
        public async ETTask CloseWindow( UIBaseView window)
        {
            string ui_name = window.GetType().Name;
            await this.CloseWindow(ui_name);
        }
        /// <summary>
        /// 关闭窗体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public async ETTask CloseWindow<T>()
        {
            string ui_name = typeof(T).Name;
            await this.CloseWindow(ui_name);
        }
        /// <summary>
        /// 关闭窗体
        /// </summary>
        /// <param name="ui_name"></param>
        public async ETTask CloseWindow( string ui_name)
        {
            var target = this.GetWindow(ui_name, 1);
            if (target == null) return;
            while (target.LoadingState != UIWindowLoadingState.LoadOver)
            {
                await TimerManager.Instance.WaitAsync(1);
            }
            this.__RemoveFromStack(target);
            this.__InnnerCloseWindow(target);
        }
        
        /// <summary>
        /// 通过层级关闭
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="except_ui_names"></param>
        public async ETTask CloseWindowByLayer( UILayerNames layer, params string[] except_ui_names)
        {
            Dictionary<string, bool> dict_ui_names = null;
            if (except_ui_names != null && except_ui_names.Length > 0)
            {
                dict_ui_names = new Dictionary<string, bool>();
                for (int i = 0; i < except_ui_names.Length; i++)
                {
                    dict_ui_names[except_ui_names[i]] = true;
                }
            }

            using (ListComponent<ETTask> TaskScheduler = ListComponent<ETTask>.Create())
            {
                foreach (var item in this.windows)
                {
                    if (item.Value.Layer == layer && (dict_ui_names == null || !dict_ui_names.ContainsKey(item.Key)))
                    {
                        TaskScheduler.Add(this.CloseWindow(item.Key));
                    }
                }
                await ETTaskHelper.WaitAll(TaskScheduler);
            }
        }
        /// <summary>
        /// 销毁窗体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public async ETTask DestroyWindow<T>() where T:UIBaseView
        {
            string ui_name = typeof(T).Name;
            await this.DestroyWindow(ui_name);
        }
        /// <summary>
        /// 销毁窗体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public async ETTask DestroyWindow( string ui_name)
        {
            var target = this.GetWindow(ui_name);
            if (target != null)
            {
                await this.CloseWindow(ui_name);
                InnerDestroyWindow(target);
                this.windows.Remove(target.Name);
                target.Dispose();
            }
        }

        /// <summary>
        /// 销毁隐藏状态的窗口
        /// </summary>
        public async ETTask DestroyUnShowWindow()
        {
            using (ListComponent<ETTask> TaskScheduler = ListComponent<ETTask>.Create())
            {
                foreach (var key in this.windows.Keys.ToList())
                {
                    if (!this.windows[key].Active)
                    {
                        TaskScheduler.Add(this.DestroyWindow(key));
                    }
                }
                await ETTaskHelper.WaitAll(TaskScheduler);
            }
        }
        /// <summary>
        /// 打开窗口 对应 AwakeSystem<T>
        /// </summary>
        /// <param name="path">预制体路径</param>
        /// <param name="layer_name">UI层级</param>
        /// <param name="banKey">是否禁止监听返回键事件</param>
        /// <typeparam name="T">要打开的窗口</typeparam>
        /// <returns></returns>
        public async ETTask<T> OpenWindow<T>( string path, 
            UILayerNames layer_name = UILayerNames.NormalLayer,bool banKey=true) where T : UIBaseView,IOnCreate
        {
            string ui_name = typeof(T).Name;
            var target = this.GetWindow(ui_name);
            if (target == null)
            {
                target = this.__InitWindow<T>(path, layer_name);
                this.windows[ui_name] = target;
            }
            target.Layer = layer_name;
            target.BanKey = banKey;
            return await this.__InnerOpenWindow<T>(target);

        }
        /// <summary>
        /// 打开窗口 对应 AwakeSystem<T,P1>
        /// </summary>
        /// <param name="path">预制体路径</param>
        /// <param name="layer_name">UI层级</param>
        /// <param name="banKey">是否禁止监听返回键事件</param>
        /// <typeparam name="T">要打开的窗口</typeparam>
        /// <returns></returns>
        public async ETTask<T> OpenWindow<T, P1>( string path, P1 p1, 
            UILayerNames layer_name = UILayerNames.NormalLayer,bool banKey=true) where T : UIBaseView,IOnCreate,IOnEnable<P1>
        {

            string ui_name = typeof(T).Name;
            var target = this.GetWindow(ui_name);
            if (target == null)
            {
                target = this.__InitWindow<T>(path, layer_name);
                this.windows[ui_name] = target;
            }
            target.Layer = layer_name;
            target.BanKey = banKey;
            return await this.__InnerOpenWindow<T, P1>(target, p1);

        }
        /// <summary>
        /// 打开窗口 对应 AwakeSystem<T,P1,P2>
        /// </summary>
        /// <param name="path">预制体路径</param>
        /// <param name="layer_name">UI层级</param>
        /// <param name="banKey">是否禁止监听返回键事件</param>
        /// <typeparam name="T">要打开的窗口</typeparam>
        /// <returns></returns>
        public async ETTask<T> OpenWindow<T, P1, P2>( string path, P1 p1, P2 p2, 
            UILayerNames layer_name = UILayerNames.NormalLayer,bool banKey=true) where T : UIBaseView,IOnCreate,IOnEnable<P1,P2>
        {

            string ui_name = typeof(T).Name;
            var target = this.GetWindow(ui_name);
            if (target == null)
            {
                target = this.__InitWindow<T>(path, layer_name);
                this.windows[ui_name] = target;
            }
            target.Layer = layer_name;
            target.BanKey = banKey;
            return await this.__InnerOpenWindow<T, P1, P2>(target, p1, p2);

        }
        /// <summary>
        /// 打开窗口 对应 AwakeSystem<T,P1,P2,P3>
        /// </summary>
        /// <param name="path">预制体路径</param>
        /// <param name="layer_name">UI层级</param>
        /// <param name="banKey">是否禁止监听返回键事件</param>
        /// <typeparam name="T">要打开的窗口</typeparam>
        /// <returns></returns>
        public async ETTask<T> OpenWindow<T, P1, P2, P3>( string path, P1 p1, P2 p2, P3 p3, 
            UILayerNames layer_name = UILayerNames.NormalLayer,bool banKey=true) where T : UIBaseView,IOnCreate,IOnEnable<P1,P2,P3>
        {

            string ui_name = typeof(T).Name;
            var target = this.GetWindow(ui_name);
            if (target == null)
            {
                target = this.__InitWindow<T>(path, layer_name);
                this.windows[ui_name] = target;
            }
            target.Layer = layer_name;
            target.BanKey = banKey;
            return await this.__InnerOpenWindow<T, P1, P2, P3>(target, p1, p2, p3);

        }
        /// <summary>
        /// 打开窗口 对应 AwakeSystem<T,P1,P2,P3,P4>
        /// </summary>
        /// <param name="path">预制体路径</param>
        /// <param name="layer_name">UI层级</param>
        /// <param name="banKey">是否禁止监听返回键事件</param>
        /// <typeparam name="T">要打开的窗口</typeparam>
        /// <returns></returns>
        public async ETTask<T> OpenWindow<T, P1, P2, P3, P4>( string path, P1 p1, P2 p2, P3 p3, P4 p4, 
            UILayerNames layer_name = UILayerNames.NormalLayer,bool banKey=true) where T : UIBaseView,IOnCreate,IOnEnable<P1,P2,P3,P4>
        {

            string ui_name = typeof(T).Name;
            var target = this.GetWindow(ui_name);
            if (target == null)
            {
                target = this.__InitWindow<T>(path, layer_name);
                this.windows[ui_name] = target;
            }
            target.Layer = layer_name;
            target.BanKey = banKey;
            return await this.__InnerOpenWindow<T, P1, P2, P3, P4>(target, p1, p2, p3, p4);

        }
        
        /// <summary>
        /// 打开窗口（返回ETTask） 对应 AwakeSystem<T>
        /// </summary>
        /// <param name="path">预制体路径</param>
        /// <param name="layer_name">UI层级</param>
        /// <typeparam name="T">要打开的窗口</typeparam>
        /// <returns></returns>
        public async ETTask OpenWindowTask<T>( string path, UILayerNames layer_name = UILayerNames.NormalLayer) where T : UIBaseView,IOnCreate,IOnEnable
        {
            await this.OpenWindow<T>(path,layer_name);
        }
        /// <summary>
        /// 打开窗口（返回ETTask）  对应 AwakeSystem<T,P1>
        /// </summary>
        /// <param name="path">预制体路径</param>
        /// <param name="layer_name">UI层级</param>
        /// <typeparam name="T">要打开的窗口</typeparam>
        /// <returns></returns>
        public async ETTask OpenWindowTask<T, P1>( string path, P1 p1, UILayerNames layer_name = UILayerNames.NormalLayer) where T : UIBaseView,IOnCreate,IOnEnable<P1>
        {
            await this.OpenWindow<T,P1>(path,p1,layer_name);
        }
        /// <summary>
        /// 打开窗口（返回ETTask）  对应 AwakeSystem<T,P1,P2>
        /// </summary>
        /// <param name="path">预制体路径</param>
        /// <param name="layer_name">UI层级</param>
        /// <typeparam name="T">要打开的窗口</typeparam>
        /// <returns></returns>
        public async ETTask OpenWindowTask<T, P1, P2>( string path, P1 p1, P2 p2, UILayerNames layer_name = UILayerNames.NormalLayer) where T : UIBaseView,IOnCreate,IOnEnable<P1,P2>
        {
            await this.OpenWindow<T, P1, P2>(path, p1, p2, layer_name);
        }
        /// <summary>
        /// 打开窗口（返回ETTask） 对应 AwakeSystem<T,P1,P2,P3>
        /// </summary>
        /// <param name="path">预制体路径</param>
        /// <param name="layer_name">UI层级</param>
        /// <typeparam name="T">要打开的窗口</typeparam>
        /// <returns></returns>
        public async ETTask OpenWindowTask<T, P1, P2, P3>( string path, P1 p1, P2 p2, P3 p3, UILayerNames layer_name = UILayerNames.NormalLayer) where T : UIBaseView,IOnCreate,IOnEnable<P1,P2,P3>
        {
            await this.OpenWindow<T, P1, P2,P3>(path, p1, p2,p3, layer_name);
        }
        /// <summary>
        /// 打开窗口（返回ETTask）  对应 AwakeSystem<T,P1,P2,P3,P4>
        /// </summary>
        /// <param name="path">预制体路径</param>
        /// <param name="layer_name">UI层级</param>
        /// <typeparam name="T">要打开的窗口</typeparam>
        /// <returns></returns>
        public async ETTask OpenWindowTask<T, P1, P2, P3, P4>( string path, P1 p1, P2 p2, P3 p3, P4 p4, UILayerNames layer_name = UILayerNames.NormalLayer) where T : UIBaseView,IOnCreate,IOnEnable<P1,P2,P3,P4>
        {
            await this.OpenWindow<T, P1, P2,P3,P4>(path, p1, p2,p3,p4, layer_name);
        }
        /// <summary>
        /// 销毁除指定窗口外所有窗口
        /// </summary>
        /// <param name="type_names">指定窗口</param>
        public async ETTask DestroyWindowExceptNames( string[] type_names = null)
        {
            Dictionary<string, bool> dict_ui_names = new Dictionary<string, bool>();
            if (type_names != null)
            {
                for (int i = 0; i < type_names.Length; i++)
                {
                    dict_ui_names[type_names[i]] = true;
                }
            }
            var keys = this.windows.Keys.ToArray();
            using (ListComponent<ETTask> TaskScheduler = ListComponent<ETTask>.Create())
            {
                for (int i = this.windows.Count - 1; i >= 0; i--)
                {
                    if (!dict_ui_names.ContainsKey(keys[i]))
                    {
                        TaskScheduler.Add(this.DestroyWindow(keys[i]));
                    }
                }
                await ETTaskHelper.WaitAll(TaskScheduler);
            }
        }
        /// <summary>
        /// 销毁指定层级外层级所有窗口
        /// </summary>
        /// <param name="layer">指定层级</param>
        public async ETTask DestroyWindowExceptLayer( UILayerNames layer)
        {
            var keys = this.windows.Keys.ToArray();
            using (ListComponent<ETTask> TaskScheduler = ListComponent<ETTask>.Create())
            {
                for (int i = this.windows.Count - 1; i >= 0; i--)
                {
                    if (this.windows[keys[i]].Layer != layer)
                    {
                        TaskScheduler.Add(this.DestroyWindow(keys[i]));
                    }
                }
                await ETTaskHelper.WaitAll(TaskScheduler);
            }
        }
        /// <summary>
        /// 销毁指定层级所有窗口
        /// </summary>
        /// <param name="layer">指定层级</param>
        public async ETTask DestroyWindowByLayer( UILayerNames layer)
        {
            var keys = this.windows.Keys.ToArray();
            using (ListComponent<ETTask> TaskScheduler = ListComponent<ETTask>.Create())
            {
                for (int i = this.windows.Count - 1; i >= 0; i--)
                {
                    if (this.windows[keys[i]].Layer == layer)
                    {
                        TaskScheduler.Add(this.DestroyWindow(this.windows[keys[i]].Name));
                    }
                }
                await ETTaskHelper.WaitAll(TaskScheduler);
            }
        }
        /// <summary>
        /// 销毁所有窗体
        /// </summary>
        public async ETTask DestroyAllWindow()
        {
            var keys = this.windows.Keys.ToArray();
            List<ETTask> TaskScheduler = new List<ETTask>();
            for (int i = keys.Length - 1; i >= 0; i--)
            {
                TaskScheduler.Add(this.DestroyWindow(this.windows[keys[i]].Name));
            }
            await ETTaskHelper.WaitAll(TaskScheduler);
        }

        /// <summary>
        /// 判断窗口是否打开
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool IsActiveWindow<T>() where T : UIBaseView
        {
            string ui_name = typeof(T).Name;
            var target = this.GetWindow(ui_name);
            if (target == null)
            {
                return false;
            }
            return target.Active;
        }
        #region 私有方法

        void InnerDestroyWindow(UIWindow target)
        {
            var view = target.View;
            if (view != null)
            {
                var obj = view.GetGameObject();
                if (obj)
                {
                    if (GameObjectPoolManager.Instance == null)
                        GameObject.Destroy(obj);
                    else
                        GameObjectPoolManager.Instance.RecycleGameObject(obj);
                }
                if (view is II18N i18n)
                    I18NManager.Instance.RemoveI18NEntity(i18n);
                view.BeforeOnDestroy();
                (view as IOnDestroy)?.OnDestroy();
            }
        }
        
        /// <summary>
        /// 初始化window
        /// </summary>
        UIWindow __InitWindow<T>( string path, UILayerNames layer_name) where T : UIBaseView
        {
            UIWindow window = UIWindow.Create();
            var type = typeof(T);
            window.Name = type.Name;
            window.Active = false;
            window.Layer = layer_name;
            window.LoadingState = UIWindowLoadingState.NotStart;
            window.PrefabPath = path;
            window.View=Activator.CreateInstance<T>();
            return window;
        }

        void __Deactivate(UIWindow target)
        {
            var view = target.View;
            if (view != null)
                view.SetActive(false);
        }

        #region 内部加载窗体,依次加载prefab、AwakeSystem、InitializationSystem、OnCreateSystem、OnEnableSystem

        void InnerResetWindowLayer(UIWindow window)
        {
            var target =window;
            var view = target.View;
            var uiTrans = view.GetTransform();
            if (uiTrans!=null)
            {
                var layer = GetLayer(target.Layer);
                uiTrans.transform.SetParent(layer.transform, false);
            }
            if (view is IOnWidthPaddingChange)
                OnWidthPaddingChange(view);
        }
        
        async ETTask<T> __InnerOpenWindow<T>( UIWindow target) where T : UIBaseView
        {
            CoroutineLock coroutineLock = null;
            try
            {
                coroutineLock = await CoroutineLockManager.Instance.Wait(CoroutineLockType.UIManager, target.GetHashCode());
                target.Active = true;
                T res = target.View as T;
                var need_load = target.LoadingState == UIWindowLoadingState.NotStart;
                target.LoadingState = UIWindowLoadingState.Loading;
                if (need_load)
                {
                    await InnerOpenWindow_GetGameObject(target.PrefabPath,target);
                }
                InnerResetWindowLayer(target);
                await this.__AddWindowToStack(target);
                target.LoadingState = UIWindowLoadingState.LoadOver;
                return res;
            }
            finally
            {
                coroutineLock?.Dispose();
            }

        }
        async ETTask<T> __InnerOpenWindow<T, P1>( UIWindow target, P1 p1) where T : UIBaseView
        {
            CoroutineLock coroutineLock = null;
            try
            {
                coroutineLock = await CoroutineLockManager.Instance.Wait(CoroutineLockType.UIManager, target.GetHashCode());
                target.Active = true;
                T res = target.View as T;
                var need_load = target.LoadingState == UIWindowLoadingState.NotStart;
                target.LoadingState = UIWindowLoadingState.Loading;
                if (need_load)
                {
                    await InnerOpenWindow_GetGameObject(target.PrefabPath,target);
                }
                InnerResetWindowLayer(target);
                await this.__AddWindowToStack(target, p1);
                target.LoadingState = UIWindowLoadingState.LoadOver;
                return res;
            }
            finally
            {
                coroutineLock?.Dispose();
            }
        }
        async ETTask<T> __InnerOpenWindow<T, P1, P2>( UIWindow target, P1 p1, P2 p2) where T : UIBaseView
        {
            CoroutineLock coroutineLock = null;
            try
            {
                coroutineLock = await CoroutineLockManager.Instance.Wait(CoroutineLockType.UIManager, target.GetHashCode());
                target.Active = true;
                T res = target.View as T;
                var need_load = target.LoadingState == UIWindowLoadingState.NotStart;
                target.LoadingState = UIWindowLoadingState.Loading;
                if (need_load)
                {
                    await InnerOpenWindow_GetGameObject(target.PrefabPath,target);
                }
                InnerResetWindowLayer(target);
                await this.__AddWindowToStack(target, p1, p2);
                target.LoadingState = UIWindowLoadingState.LoadOver;
                return res;
            }
            finally
            {
                coroutineLock?.Dispose();
            }
        }
        async ETTask<T> __InnerOpenWindow<T, P1, P2, P3>( UIWindow target, P1 p1, P2 p2, P3 p3) where T : UIBaseView
        {
            CoroutineLock coroutineLock = null;
            try
            {
                coroutineLock = await CoroutineLockManager.Instance.Wait(CoroutineLockType.UIManager, target.GetHashCode());
                target.Active = true;
                T res = target.View as T;
                var need_load = target.LoadingState == UIWindowLoadingState.NotStart;
                target.LoadingState = UIWindowLoadingState.Loading;
                if (need_load)
                {
                    await InnerOpenWindow_GetGameObject(target.PrefabPath,target);
                }
                InnerResetWindowLayer(target);
                await this.__AddWindowToStack(target, p1, p2, p3);
                target.LoadingState = UIWindowLoadingState.LoadOver;
                return res;
            }
            finally
            {
                coroutineLock?.Dispose();
            }
        }
        async ETTask<T> __InnerOpenWindow<T, P1, P2, P3, P4>( UIWindow target, P1 p1, P2 p2, P3 p3, P4 p4) where T : UIBaseView
        {
            CoroutineLock coroutineLock = null;
            try
            {
                coroutineLock = await CoroutineLockManager.Instance.Wait(CoroutineLockType.UIManager, target.GetHashCode());
                target.Active = true;
                T res = target.View as T;
                var need_load = target.LoadingState == UIWindowLoadingState.NotStart;
                target.LoadingState = UIWindowLoadingState.Loading;
                if (need_load)
                {
                    await InnerOpenWindow_GetGameObject(target.PrefabPath,target);
                }
                InnerResetWindowLayer(target);
                await this.__AddWindowToStack(target, p1, p2, p3, p4);
                target.LoadingState = UIWindowLoadingState.LoadOver;
                return res;
            }
            finally
            {
                coroutineLock?.Dispose();
            }
        }

        async ETTask InnerOpenWindow_GetGameObject(string path,UIWindow target)
        {
            var view = target.View;
	        
            // await UIWatcherComponent.Instance.OnViewInitializationSystem(view);
            
            var go = await GameObjectPoolManager.Instance.GetGameObjectAsync(path);
            if (go == null)
            {
                Log.Error(string.Format("UIManager InnerOpenWindow {0} faild", target.PrefabPath));
                return;
            }
            var trans = go.transform;
            trans.SetParent(GetLayer(target.Layer).transform, false);
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
         void __InnnerCloseWindow( UIWindow target)
        {
            if (target.Active)
            {
                __Deactivate(target);
                target.Active = false;
            }
        }
        /// <summary>
        /// 将窗口移到当前层级最上方
        /// </summary>
        
        /// <typeparam name="T"></typeparam>
        public void MoveWindowToTop<T>() where T:UIBaseView
        {
            string ui_name = typeof(T).Name;
            var target = this.GetWindow(ui_name,1);
            if (target == null)
            {
               return;
            }
            var layer_name = target.Layer;
            if (this.window_stack[layer_name].Contains(ui_name))
            {
                this.window_stack[layer_name].Remove(ui_name);
            }
            this.window_stack[layer_name].AddFirst(ui_name);
            InnerAddWindowToStack(target);
        }
        async ETTask __AddWindowToStack( UIWindow target)
        {
            var ui_name = target.Name;
            var layer_name = target.Layer;
            bool isFirst = true;
            if (this.window_stack[layer_name].Contains(ui_name))
            {
                isFirst = false;
                this.window_stack[layer_name].Remove(ui_name);
            }
            this.window_stack[layer_name].AddFirst(ui_name);
            InnerAddWindowToStack(target);
            var view = target.View;
            view.SetActive(true);
            if (isFirst && (layer_name == UILayerNames.BackgroudLayer || layer_name == UILayerNames.GameBackgroudLayer))
            {
                //如果是背景layer，则销毁所有的normal层|BackgroudLayer
                await this.CloseWindowByLayer(UILayerNames.NormalLayer);
                await this.CloseWindowByLayer(UILayerNames.GameLayer);
                await this.CloseWindowByLayer(UILayerNames.BackgroudLayer, ui_name);
                await this.CloseWindowByLayer(UILayerNames.GameBackgroudLayer, ui_name);
            }
        }
        async ETTask __AddWindowToStack<P1>( UIWindow target, P1 p1)
        {
            var ui_name = target.Name;
            var layer_name = target.Layer;
            bool isFirst = true;
            if (this.window_stack[layer_name].Contains(ui_name))
            {
                isFirst = false;
                this.window_stack[layer_name].Remove(ui_name);
            }
            this.window_stack[layer_name].AddFirst(ui_name);
            InnerAddWindowToStack(target);
            var view = target.View;
            view.SetActive(true, p1);
            if (isFirst && (layer_name == UILayerNames.BackgroudLayer || layer_name == UILayerNames.GameBackgroudLayer))
            {
                //如果是背景layer，则销毁所有的normal层|BackgroudLayer
                await this.CloseWindowByLayer(UILayerNames.NormalLayer);
                await this.CloseWindowByLayer(UILayerNames.GameLayer);
                await this.CloseWindowByLayer(UILayerNames.BackgroudLayer, ui_name);
                await this.CloseWindowByLayer(UILayerNames.GameBackgroudLayer, ui_name);
            }
        }
        async ETTask __AddWindowToStack<P1, P2>( UIWindow target, P1 p1, P2 p2)
        {
            var ui_name = target.Name;
            var layer_name = target.Layer;
            bool isFirst = true;
            if (this.window_stack[layer_name].Contains(ui_name))
            {
                isFirst = false;
                this.window_stack[layer_name].Remove(ui_name);
            }
            this.window_stack[layer_name].AddFirst(ui_name);
            InnerAddWindowToStack(target);
            var view = target.View;
            view.SetActive(true, p1, p2);
            if (isFirst && (layer_name == UILayerNames.BackgroudLayer || layer_name == UILayerNames.GameBackgroudLayer))
            {
                //如果是背景layer，则销毁所有的normal层|BackgroudLayer
                await this.CloseWindowByLayer(UILayerNames.NormalLayer);
                await this.CloseWindowByLayer(UILayerNames.GameLayer);
                await this.CloseWindowByLayer(UILayerNames.BackgroudLayer, ui_name);
                await this.CloseWindowByLayer(UILayerNames.GameBackgroudLayer, ui_name);
            }
        }
        async ETTask __AddWindowToStack<P1, P2, P3>( UIWindow target, P1 p1, P2 p2, P3 p3)
        {
            var ui_name = target.Name;
            var layer_name = target.Layer;
            bool isFirst = true;
            if (this.window_stack[layer_name].Contains(ui_name))
            {
                isFirst = false;
                this.window_stack[layer_name].Remove(ui_name);
            }
            this.window_stack[layer_name].AddFirst(ui_name);
            InnerAddWindowToStack(target);
            var view = target.View;
            view.SetActive(true, p1, p2, p3);
            if (isFirst && (layer_name == UILayerNames.BackgroudLayer || layer_name == UILayerNames.GameBackgroudLayer))
            {
                //如果是背景layer，则销毁所有的normal层|BackgroudLayer
                await this.CloseWindowByLayer(UILayerNames.NormalLayer);
                await this.CloseWindowByLayer(UILayerNames.GameLayer);
                await this.CloseWindowByLayer(UILayerNames.BackgroudLayer, ui_name);
                await this.CloseWindowByLayer(UILayerNames.GameBackgroudLayer, ui_name);
            }
        }
        async ETTask __AddWindowToStack<P1, P2, P3, P4>( UIWindow target, P1 p1, P2 p2, P3 p3, P4 p4)
        {
            var ui_name = target.Name;
            var layer_name = target.Layer;
            bool isFirst = true;
            if (this.window_stack[layer_name].Contains(ui_name))
            {
                isFirst = false;
                this.window_stack[layer_name].Remove(ui_name);
            }
            this.window_stack[layer_name].AddFirst(ui_name);
            InnerAddWindowToStack(target);
            var view = target.View;
            view.SetActive(true, p1, p2, p3, p4);
            if (isFirst && (layer_name == UILayerNames.BackgroudLayer || layer_name == UILayerNames.GameBackgroudLayer))
            {
                //如果是背景layer，则销毁所有的normal层|BackgroudLayer
                await this.CloseWindowByLayer(UILayerNames.NormalLayer);
                await this.CloseWindowByLayer(UILayerNames.GameLayer);
                await this.CloseWindowByLayer(UILayerNames.BackgroudLayer, ui_name);
                await this.CloseWindowByLayer(UILayerNames.GameBackgroudLayer, ui_name);
            }
        }

        void InnerAddWindowToStack(UIWindow target)
        {
            var view = target.View;
            var uiTrans = view.GetTransform();
            if (uiTrans!=null)
            {
                uiTrans.SetAsLastSibling();
            }
        }
        /// <summary>
        /// 移除
        /// </summary>
        
        /// <param name="target"></param>
        void __RemoveFromStack( UIWindow target)
        {
            var ui_name = target.Name;
            var layer_name = target.Layer;
            if (this.window_stack.ContainsKey(layer_name))
            {
                this.window_stack[layer_name].Remove(ui_name);
            }
            else
            {
                Log.Error("not layer, name :" + layer_name);
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
            foreach (var layer in this.window_stack.Values)
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
            return UILayersManager.Instance.UICamera;
        }
        
        public UIBaseView GetView(string ui_name)
        {
            var res = this.GetWindow(ui_name);
            if (res != null)
            {
                return res.View;
            }
            return null;
        }

        public UILayer GetLayer(UILayerNames layer)
        {
            if(UILayersManager.Instance.layers.TryGetValue(layer,out var res))
            {
                return res;
            }
            return null;
        }

       
        #endregion
    }
}