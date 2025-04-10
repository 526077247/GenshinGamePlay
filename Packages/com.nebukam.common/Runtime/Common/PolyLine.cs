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

    public interface IPolyLine<out V> : IVertexGroup<V>
        where V : IVertex
    {

    }

    public interface IEditablePolyLine<out V> : IPolyLine<V>
        where V : IVertex
    {

    }

    public class PolyLine<V> : VertexGroup<V>, IEditablePolyLine<V>
        where V : Vertex, IVertex, new()
    {

        public PolyLine() : base()
        {

        }


        #region utils

        /// <summary>
        /// Insert new vertex between existing vertices based on their distance and the given subSegmentLength value.
        /// </summary>
        /// <param name="subSegmentLength"></param>
        public void Subdivide(float subSegmentLength)
        {

#if UNITY_EDITOR
            if (subSegmentLength <= 0.0f ) { throw new System.Exception("Subdivide(float) : parameter subSegmentLength must be > 0.0f"); }
#endif


            int count = m_vertices.Count - 1, insertIndex = 0;

            for (int i = 0; i < count; i++)
            {

                float3 v = m_vertices[insertIndex].pos, v_next = m_vertices[insertIndex + 1].pos;
                float dist = math.distance(v, v_next), subDist = dist / subSegmentLength;

                insertIndex++;

                if (subDist <= 1.0f) { continue; }

                int steps = ((int)math.ceil(dist / subSegmentLength))-1;
                float3 dir = math.normalize(v_next - v);

                v = v + dir * ((dist - ((steps-1) * subSegmentLength)) * 0.5f);

                for (int j = 0; j < steps; j++)
                {
                    Insert(insertIndex, v + dir * (j * subSegmentLength));
                    insertIndex++;
                }

            }

        }

        #endregion

    }

}
