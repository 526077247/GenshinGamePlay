using System;
using System.Collections.Generic;

namespace TaoTie
{
    public interface IScene
    {
        public string GetName();
        public string GetScenePath();
        /// <summary>
        /// 获取各阶段进度比例
        /// </summary>
        public void GetProgressPercent(out float cleanup, out float loadScene, out float prepare);
        public string[] GetDontDestroyWindow();

        /// <summary>
        /// 场景切换中不释放，切换完毕后释放的资源列表
        /// </summary>
        /// <returns></returns>
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
        public ETTask OnPrepare(float progressStart,float progressEnd);

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