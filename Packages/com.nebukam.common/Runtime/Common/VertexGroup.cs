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
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Nebukam.Common
{

    public interface IVertexGroup<out V>
        where V : IVertex
    {

        int Count { get; }
        V this[int index] { get; }
        int this[IVertex v] { get; }


        #region Nearest vertex in group

        int GetNearestVertexIndex(IVertex v);
        V GetNearestVertex(IVertex v);
        int GetNearestVertexIndex(float3 v);
        V GetNearestVertex(float3 v);

        #endregion

    }

    public interface IClearableVertexGroup<out V> : IVertexGroup<V>
        where V : IVertex
    {
        void Clear(bool release = false);
    }

    public interface IEditableVertexGroup<out V> : IClearableVertexGroup<V>
        where V : IVertex
    {

        #region add

        V Add();
        V Add(IVertex v);
        V Add(float3 v);
        V Insert(int index, IVertex v);
        V Insert(int index, float3 v);

        #endregion

        #region remove
        
        V Remove(IVertex v, bool release = false);
        V RemoveAt(int index, bool release = false);

        #endregion

        #region utils
        
        void Reverse();
        V Shift(bool release = false);
        V Pop(bool release = false);
        void Offset(float3 offset);

        #endregion
        
    }

    public class VertexGroup<V> : PoolItem, IEditableVertexGroup<V>
        where V : Vertex, IVertex, new()
    {

        protected Pool.OnItemReleased m_onVertexReleasedCached;

        protected bool m_locked = false;
        public bool locked { get { return m_locked; } }

        protected List<IVertex> m_vertices = new List<IVertex>();
        public List<IVertex> vertices { get { return m_vertices; } }

        public int Count { get { return m_vertices.Count; } }

        public V this[int index] { get { return m_vertices[index] as V; } }
        public int this[IVertex v] { get { return m_vertices.IndexOf(v); } }

        public VertexGroup()
        {
            m_onVertexReleasedCached = OnVertexReleased;
        }


        #region add
        
        /// <summary>
        /// Create a vertex in the group
        /// </summary>
        /// <returns></returns>
        public virtual V Add()
        {
            return Add(Pool.Rent<V>() as IVertex);
        }

        /// <summary>
        /// Adds a vertex in the group.
        /// </summary>
        /// <param name="v">The vertex to be added.</param>
        /// <param name="ownVertex">Whether or not this group gets ownership over the vertex.</param>
        /// <returns></returns>
        public V Add(IVertex v)
        {
            V vert = v as V;
#if UNITY_EDITOR
            if (vert == null) { throw new System.Exception("Wrong vertex type"); }
#endif
            if (m_vertices.Contains(vert)) { return vert; }
            m_vertices.Add(vert);
            OnVertexAdded(vert);
            return vert;
        }

        /// <summary>
        /// Create a vertex in the group, from a float3.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public virtual V Add(float3 v)
        {
            V vert = Pool.Rent<V>();
            vert.pos = v;
            return Add(vert as IVertex);
        }

        /// <summary>
        /// Inserts a vertex at a given index in the group.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="v"></param>
        /// <param name="ownVertex"></param>
        /// <param name="allowProxy"></param>
        /// <returns></returns>
        public V Insert(int index, IVertex v)
        {

#if UNITY_EDITOR
            if (!(v is V)) { throw new System.Exception("Insert(float, IVertex) : parameter T (" + v.GetType().Name + ") does not implement " + typeof(V).Name + "."); }
#endif
            V vert = v as V;

            int currentIndex = m_vertices.IndexOf(v);
            if(currentIndex == index) { return vert; }

            if (currentIndex != -1)
            {

                m_vertices.RemoveAt(currentIndex);

                if (currentIndex < index)
                    m_vertices.Insert(index - 1, vert);
                else
                    m_vertices.Insert(index, vert);
                
            }
            else
            {
                //Add vertex
                m_vertices.Insert(index, vert);
                OnVertexAdded(vert);                
            }

            return vert;

        }

        /// <summary>
        /// Create a vertex in the group at the given index
        /// </summary>
        /// <param name="index"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public virtual V Insert(int index, float3 v)
        {
            V vert = Pool.Rent<V>();
            vert.pos = v;

            m_vertices.Insert(index, vert);
            OnVertexAdded(vert);
            return vert;
        }

#endregion

        #region remove

        /// <summary>
        /// Removes a given vertex from the group.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="keepProxies"></param>
        /// <returns></returns>
        public V Remove(IVertex v, bool release = false)
        {
            int index = m_vertices.IndexOf(v);
            return RemoveAt(index);
        }

        /// <summary>
        /// Removes the vertex at the given index from the group .
        /// </summary>
        /// <param name="index"></param>
        /// <param name="keepProxies"></param>
        /// <returns></returns>
        public V RemoveAt(int index, bool release = false)
        {
            V result = m_vertices[index] as V;
            m_vertices.RemoveAt(index);
            OnVertexRemoved(result as V);
            if (release) { result.Release(); }
            return result;
        }

        #endregion

        #region callbacks

        protected virtual void OnVertexAdded(V v)
        {
            //v.OnRelease(m_onVertexReleasedCached);
        }

        protected virtual void OnVertexRemoved(V v)
        {
            //v.OffRelease(m_onVertexReleasedCached);
        }

        protected virtual void OnVertexReleased(IPoolItem vertex)
        {
            Remove(vertex as V);
        }

        #endregion

        #region utils

        /// <summary>
        /// Inverse vertices's order
        /// </summary>
        public void Reverse()
        {
            m_vertices.Reverse();
        }

        /// <summary>
        /// Removes and return the first item in the group
        /// </summary>
        /// <returns></returns>
        public V Shift(bool release = false)
        {
            int count = m_vertices.Count;
            if (count == 0) { return null; }
            return RemoveAt(0, release);
        }

        /// <summary>
        /// Removes and return the last item in the group
        /// </summary>
        /// <returns></returns>
        public V Pop(bool release = false)
        {
            int count = m_vertices.Count;
            if (count == 0) { return null; }
            return RemoveAt(count - 1, release);
        }

        /// <summary>
        /// Removes all vertices from the group.
        /// </summary>
        public virtual void Clear(bool release = false)
        {
            int count = m_vertices.Count;
            while (count != 0)
            {
                RemoveAt(count - 1, release);
                count = m_vertices.Count;
            }
        }

        /// <summary>
        /// Offset all vertices
        /// </summary>
        /// <param name="offset"></param>
        public void Offset(float3 offset)
        {
            for (int i = 0, count = m_vertices.Count; i < count; i++)
                m_vertices[i].pos += offset;
        }

        #endregion

        #region PoolItem

        protected virtual void CleanUp()
        {
            Clear(false);
        }

        #endregion

        #region Nearest vertex in group

        /// <summary>
        /// Return the vertex index in group of the nearest IVertex to a given IVertex v
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public int GetNearestVertexIndex(IVertex v)
        {
            int index = -1, count = m_vertices.Count;
            float dist, sDist = float.MaxValue;
            float3 A = v.pos, B, C;
            IVertex oV;
            for (int i = 0; i < count; i++)
            {
                oV = m_vertices[i];

                if (oV == v) { continue; }

                B = oV.pos;
                C = float3(A.x - B.x, A.y - B.y, A.z - B.z);
                dist = C.x * C.x + C.y * C.y + C.z * C.z;

                if (dist > sDist)
                {
                    sDist = dist;
                    index = i;
                }
            }

            return index;
        }

        /// <summary>
        /// Return the the nearest IVertex in group to a given IVertex v
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public V GetNearestVertex(IVertex v)
        {
            int index = GetNearestVertexIndex(v);
            if (index == -1) { return null; }
            return m_vertices[index] as V;
        }

        /// <summary>
        /// Return the vertex index in group of the nearest IVertex to a given v
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public int GetNearestVertexIndex(float3 v)
        {
            int index = -1, count = m_vertices.Count;
            float dist, sDist = float.MaxValue;
            float3 B, C;
            for (int i = 0; i < count; i++)
            {
                B = m_vertices[i].pos;
                C = float3(v.x - B.x, v.y - B.y, v.z - B.z);
                dist = C.x * C.x + C.y * C.y + C.z * C.z;

                if (dist > sDist)
                {
                    sDist = dist;
                    index = i;
                }
            }

            return index;
        }

        /// <summary>
        /// Return the nearest IVertex in group of the nearest IVertex to a given v
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public V GetNearestVertex(float3 v)
        {
            int index = GetNearestVertexIndex(v);
            if (index == -1) { return null; }
            return m_vertices[index] as V;
        }



        #endregion

    }

}
