using System.Collections.Generic;

namespace TaoTie
{
    public interface IScene
    {
        public string GetScenePath();

        public string[] GetDontDestroyWindow();

        public List<string> GetScenesChangeIgnoreClean();
        /// <summary>
        /// 创建：初始化一些需要全局保存的状态
        /// </summary>
        public ETTask OnCreate();

        /// <summary>
        /// 加载前的初始化
        /// </summary>
        public ETTask OnEnter();

        /// <summary>
        /// 设置进度
        /// </summary>
        public ETTask SetProgress(float value);
        /// <summary>
        /// 场景加载结束：后续资源准备（预加载等）
        /// </summary>
        /// <returns></returns>
        public ETTask OnPrepare();

        /// <summary>
        /// 场景加载完毕
        /// </summary>
        public ETTask OnComplete();
        
        /// <summary>
        /// 离开场景：清理场景资源
        /// </summary>
        public ETTask OnLeave();

        /// <summary>
        /// 转场景结束
        /// </summary>
        public ETTask OnSwitchSceneEnd();
    }
}