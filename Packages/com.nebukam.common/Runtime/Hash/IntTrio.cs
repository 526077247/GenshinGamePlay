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
using Unity.Burst;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Nebukam
{

    /// <summary>
    /// Trio of int with unsigned HashCode.
    /// Individual int values must range between -999 & 999. Collision occur with higher values.
    /// </summary>
    [BurstCompile]
    public struct IntTrio : System.IEquatable<IntTrio>
    {

        public static IntTrio zero = new IntTrio(0, 0, 0);
        public int x, y, z;
        public byte d;

        public IntTrio(int x, int y, int z)
        {
            d = 0;
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public IntTrio(int x) : this(x, x, x) { }
        public IntTrio(int x, int y) : this(x, y, 0) { }

        public bool Contains(int i)
        {
            return (x == i || y == i || z == i);
        }

        public static bool operator !=(IntTrio e1, IntTrio e2)
        {
            return !(e1.x == e2.x && e1.y == e2.y && e1.z == e2.z);
        }

        public static bool operator ==(IntTrio e1, IntTrio e2)
        {
            return (e1.x == e2.x && e1.y == e2.y && e1.z == e2.z);
        }

        public bool Equals(IntTrio e)
        {
            return this == e;
        }

        public override bool Equals(object obj)
        {
            IntTrio e = (IntTrio)obj;
            return (x == e.x && y == e.y && z == e.z);
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                return x * 1000000 + y * 1000 + z;
            }
        }

        public override string ToString()
        {
            return string.Format("({0},{1},{2})", x, y, z);
        }

        public static IntTrio operator +(IntTrio l, int r) { return new IntTrio(l.x + r, l.y + r, l.z + r); }
        public static IntTrio operator -(IntTrio l, int r) { return new IntTrio(l.x - r, l.y - r, l.z + r); }
        public static IntTrio operator *(IntTrio l, int r) { return new IntTrio(l.x * r, l.y * r, l.z * r); }
        public static IntTrio operator /(IntTrio l, int r) { return new IntTrio(l.x / r, l.y / r, l.z / r); }

        public static IntTrio operator +(int l, IntTrio r) { return new IntTrio(r.x + l, r.y + l, r.z + l); }
        public static IntTrio operator -(int l, IntTrio r) { return new IntTrio(r.x - l, r.y - l, r.z + l); }
        public static IntTrio operator *(int l, IntTrio r) { return new IntTrio(r.x * l, r.y * l, r.z * l); }
        public static IntTrio operator /(int l, IntTrio r) { return new IntTrio(r.x / l, r.y / l, r.z / l); }

        public static IntTrio operator +(IntTrio l, IntTrio r) { return new IntTrio(l.x + r.x, l.y + r.y, l.z + r.z); }
        public static IntTrio operator -(IntTrio l, IntTrio r) { return new IntTrio(l.x - r.x, l.y - r.y, l.z - r.z); }
        public static IntTrio operator *(IntTrio l, IntTrio r) { return new IntTrio(l.x * r.x, l.y * r.y, l.z * r.z); }
        public static IntTrio operator /(IntTrio l, IntTrio r) { return new IntTrio(l.x / r.x, l.y / r.y, l.z / r.z); }

        public static IntTrio operator +(IntTrio l, int3 r) { return new IntTrio(l.x + r.x, l.y + r.y, l.z + r.z); }
        public static IntTrio operator -(IntTrio l, int3 r) { return new IntTrio(l.x - r.x, l.y - r.y, l.z - r.z); }
        public static IntTrio operator *(IntTrio l, int3 r) { return new IntTrio(l.x * r.x, l.y * r.y, l.z * r.z); }
        public static IntTrio operator /(IntTrio l, int3 r) { return new IntTrio(l.x / r.x, l.y / r.y, l.z / r.z); }

        public static IntTrio operator +(int3 l, IntTrio r) { return new IntTrio(l.x + r.x, l.y + r.y, l.z + r.z); }
        public static IntTrio operator -(int3 l, IntTrio r) { return new IntTrio(l.x - r.x, l.y - r.y, l.z - r.z); }
        public static IntTrio operator *(int3 l, IntTrio r) { return new IntTrio(l.x * r.x, l.y * r.y, l.z * r.z); }
        public static IntTrio operator /(int3 l, IntTrio r) { return new IntTrio(l.x / r.x, l.y / r.y, l.z / r.z); }

        public static IntTrio operator +(IntTrio l, UIntTrio r) { return new IntTrio(l.x + r.x, l.y + r.y, l.z + r.z); }
        public static IntTrio operator -(IntTrio l, UIntTrio r) { return new IntTrio(l.x - r.x, l.y - r.y, l.z - r.z); }
        public static IntTrio operator *(IntTrio l, UIntTrio r) { return new IntTrio(l.x * r.x, l.y * r.y, l.z * r.z); }
        public static IntTrio operator /(IntTrio l, UIntTrio r) { return new IntTrio(l.x / r.x, l.y / r.y, l.z / r.z); }

        public static IntTrio operator +(UIntTrio l, IntTrio r) { return new IntTrio(l.x + r.x, l.y + r.y, l.z + r.z); }
        public static IntTrio operator -(UIntTrio l, IntTrio r) { return new IntTrio(l.x - r.x, l.y - r.y, l.z - r.z); }
        public static IntTrio operator *(UIntTrio l, IntTrio r) { return new IntTrio(l.x * r.x, l.y * r.y, l.z * r.z); }
        public static IntTrio operator /(UIntTrio l, IntTrio r) { return new IntTrio(l.x / r.x, l.y / r.y, l.z / r.z); }

        public static float3 operator *(IntTrio l, float3 r) { return float3(l.x * r.x, l.y * r.y, l.z * r.z); }
        public static float3 operator *(float3 l, IntTrio r) { return float3(l.x * r.x, l.y * r.y, l.z * r.z); }

        public static float3 operator *(IntTrio l, float r) { return float3(l.x * r, l.y * r, l.z * r); }
        public static float3 operator /(IntTrio l, float r) { return float3(l.x / r, l.y / r, l.z / r); }

        public static float3 operator *(float l, IntTrio r) { return float3(l * r.x, l * r.y, l * r.z); }
        public static float3 operator /(float l, IntTrio r) { return float3(l / r.x, l / r.y, l / r.z); }

        public static implicit operator IntTrio(UIntTrio trio) { return new IntTrio(trio.x, trio.y, trio.z); }
        public static implicit operator int3(IntTrio trio) { return new int3(trio.x, trio.y, trio.z); }
        public static implicit operator IntTrio(int3 i) { return new IntTrio(i.x, i.y, i.z); }
        

    }

}