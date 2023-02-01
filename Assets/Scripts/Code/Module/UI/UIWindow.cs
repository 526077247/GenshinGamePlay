using System;
namespace TaoTie
{
    public enum UIWindowLoadingState:byte
    {
        NotStart, // 未开始
        Loading, //加载中
        LoadOver, //加载完成
    }
    public class UIWindow:IDisposable
    {
        /// <summary>
        /// 窗口名字
        /// </summary>
        public string Name;
        /// <summary>
        /// 是否激活
        /// </summary>
        public bool Active;
        /// <summary>
        /// 是否正在加载
        /// </summary>
        public UIWindowLoadingState LoadingState;
        /// <summary>
        /// 预制体路径
        /// </summary>
        public string PrefabPath;
        /// <summary>
        /// 窗口层级
        /// </summary>
        public UILayerNames Layer;

        /// <summary>
        /// 窗口类型
        /// </summary>
        public UIBaseView View;
        /// <summary>
        /// 禁止物理按键
        /// </summary>
        public bool BanKey;

        public static UIWindow Create()
        {
            return ObjectPool.Instance.Fetch(TypeInfo<UIWindow>.Type) as UIWindow;
        }

        public void Dispose()
        {
            Name = null;
            Active = false;
            LoadingState = UIWindowLoadingState.NotStart;
            PrefabPath = null;
            Layer = UILayerNames.BackgroudLayer;
            View = null;
            BanKey = false;
            ObjectPool.Instance.Recycle(this);
        }
    }
}