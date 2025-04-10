// Copyright (c) 2021 Timothé Lapetite - nebukam@gmail.com
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace Nebukam
{

    public interface IPoolablePrefabItem : IPoolItem
    {
        GameObject instanceOf { get; }
    }

    internal interface IPoolablePrefabNode : IPoolablePrefabItem
    {
        GameObject __instanceOf { set; }
        IPoolablePrefabNode __prevNode { get; set; }
        bool __released { get; set; }
    }

    [DisallowMultipleComponent]
    public class PoolablePrefab : MonoBehaviour, IPoolablePrefabNode
    {

        private Type __m_type;


        [SerializeField][HideInInspector]
        internal GameObject m_instanceOf = null;

        internal Type __type { get { return __m_type == null ? __m_type = GetType() : __m_type; } }
        public GameObject instanceOf { get { return m_instanceOf; } }
        GameObject IPoolablePrefabNode.__instanceOf { set => m_instanceOf = value; }


        internal List<Prefabs.OnPrefabReleased> __onRelease = null;
        IPoolablePrefabNode IPoolablePrefabNode.__prevNode { get; set; } = null;
        bool IPoolablePrefabNode.__released { get; set; } = false;

        /// <summary>
        /// 
        /// </summary>
        public void Release()
        {
            Prefabs.Return(this);
        }

        public static implicit operator bool(PoolablePrefab item)
        {
            if(item == null) { return false; }
            return !(item as IPoolablePrefabNode).__released;
        }


        private void OnValidate()
        {
#if UNITY_EDITOR
            if (Application.isPlaying) { return; }
            //Allows hand-created instances of a poolable prefab to be returned to their pool
            m_instanceOf = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);
#endif
        }

    }

}
