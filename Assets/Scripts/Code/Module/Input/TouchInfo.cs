using System;
using UnityEngine;

namespace TaoTie
{
    public class TouchInfo : IDisposable
    {
        public int Index;
        /// <summary>
        /// 起始触碰点是否在UI上
        /// </summary>
        public bool IsStartOverUI;
        
        public Touch? Touch
        {
            get
            {
                if (Index < Input.touchCount)
                {
                    return Input.GetTouch(Index);
                }
                return null;
            }
        }

        /// <summary>
        /// 是否是模拟器下滚轮
        /// </summary>
        public bool IsScroll;
        
        public void Dispose()
        {
            Index = -1;
            IsStartOverUI = false;
            IsScroll = false;
        }
    }
}