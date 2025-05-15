using System;
using UnityEngine;

namespace TaoTie
{
    public class TouchInfo : IDisposable
    {
        public int Index;
        /// <summary>
        /// 起始点是否在UI
        /// </summary>
        public bool IsStartOverUI;
        
        public Touch Touch => Input.GetTouch(Index);

        public bool isScroll;
        
        public void Dispose()
        {
            Index = -1;
            IsStartOverUI = false;
            isScroll = false;
        }
    }
}