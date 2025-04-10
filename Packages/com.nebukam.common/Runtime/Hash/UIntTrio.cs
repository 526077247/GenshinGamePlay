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
    /// Pair of int with unsigned HashCode.
    /// Individual int values must range between -999 & 999. Collision occur with higher values.
    /// </summary>
    [BurstCompile]
    public struct UIntTrio : System.IEquatable<UIntTrio>
    {

        public static UIntTrio zero = new UIntTrio(0, 0, 0);
        public int x, y, z;
        public byte d;

        public UIntTrio(int x, int y, int z)
        {
            d = 0;
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public UIntTrio(int x) : this(x, x, x) { }
        public UIntTrio(int x, int y) : this(x, y, 0) { }

        public bool Contains(int i)
        {
            return (x == i || y == i || z == i);
        }

        public static bool operator !=(UIntTrio e1, UIntTrio e2)
        {
            return !(e1 == e2);
        }

        public static bool operator ==(UIntTrio e1, UIntTrio e2)
        {
            return ((e1.x == e2.x && e1.y == e2.y && e1.z == e2.z)
                || (e1.x == e2.x && e1.y == e2.z && e1.z == e2.y)
                || (e1.x == e2.y && e1.y == e2.x && e1.z == e2.z)
                || (e1.x == e2.y && e1.y == e2.z && e1.z == e2.x)
                || (e1.x == e2.z && e1.y == e2.x && e1.z == e2.y)
                || (e1.x == e2.z && e1.y == e2.y && e1.z == e2.x));
        }

        public bool Equals(UIntTrio e)
        {
            return this == e;
        }

        public override bool Equals(object obj)
        {
            UIntTrio e = (UIntTrio)obj;
            return this == e;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int m = max(x, y); m = max(m, z);
                int second, third;
                if (m == x)
                {
                    if (y > z) { second = y; third = z; }
                    else { second = z; third = y; }
                }
                else if (m == y)
                {
                    if (x > z) { second = x; third = z; }
                    else { second = z; third = x; }
                }
                else
                {
                    if (x > y) { second = x; third = z; }
                    else { second = z; third = x; }
                }

                return m * 1000000 + second * 1000 + third;
            }
        }

        public override string ToString()
        {
            return string.Format("({0},{1},{2})", x, y, z);
        }

        public static UIntTrio operator +(UIntTrio l, int r) { return new UIntTrio(l.x + r, l.y + r, l.z + r); }
        public static UIntTrio operator -(UIntTrio l, int r) { return new UIntTrio(l.x - r, l.y - r, l.z + r); }
        public static UIntTrio operator *(UIntTrio l, int r) { return new UIntTrio(l.x * r, l.y * r, l.z * r); }
        public static UIntTrio operator /(UIntTrio l, int r) { return new UIntTrio(l.x / r, l.y / r, l.z / r); }

        public static UIntTrio operator +(int l, UIntTrio r) { return new UIntTrio(r.x + l, r.y + l, r.z + l); }
        public static UIntTrio operator -(int l, UIntTrio r) { return new UIntTrio(r.x - l, r.y - l, r.z + l); }
        public static UIntTrio operator *(int l, UIntTrio r) { return new UIntTrio(r.x * l, r.y * l, r.z * l); }
        public static UIntTrio operator /(int l, UIntTrio r) { return new UIntTrio(r.x / l, r.y / l, r.z / l); }

        public static UIntTrio operator +(UIntTrio l, UIntTrio r) { return new UIntTrio(l.x + r.x, l.y + r.y, l.z + r.z); }
        public static UIntTrio operator -(UIntTrio l, UIntTrio r) { return new UIntTrio(l.x - r.x, l.y - r.y, l.z - r.z); }
        public static UIntTrio operator *(UIntTrio l, UIntTrio r) { return new UIntTrio(l.x * r.x, l.y * r.y, l.z * r.z); }
        public static UIntTrio operator /(UIntTrio l, UIntTrio r) { return new UIntTrio(l.x / r.x, l.y / r.y, l.z / r.z); }

        public static UIntTrio operator +(UIntTrio l, int3 r) { return new UIntTrio(l.x + r.x, l.y + r.y, l.z + r.z); }
        public static UIntTrio operator -(UIntTrio l, int3 r) { return new UIntTrio(l.x - r.x, l.y - r.y, l.z - r.z); }
        public static UIntTrio operator *(UIntTrio l, int3 r) { return new UIntTrio(l.x * r.x, l.y * r.y, l.z * r.z); }
        public static UIntTrio operator /(UIntTrio l, int3 r) { return new UIntTrio(l.x / r.x, l.y / r.y, l.z / r.z); }

        public static UIntTrio operator +(int3 l, UIntTrio r) { return new UIntTrio(l.x + r.x, l.y + r.y, l.z + r.z); }
        public static UIntTrio operator -(int3 l, UIntTrio r) { return new UIntTrio(l.x - r.x, l.y - r.y, l.z - r.z); }
        public static UIntTrio operator *(int3 l, UIntTrio r) { return new UIntTrio(l.x * r.x, l.y * r.y, l.z * r.z); }
        public static UIntTrio operator /(int3 l, UIntTrio r) { return new UIntTrio(l.x / r.x, l.y / r.y, l.z / r.z); }

        public static UIntTrio operator +(IntTrio l, UIntTrio r) { return new UIntTrio(l.x + r.x, l.y + r.y, l.z + r.z); }
        public static UIntTrio operator -(IntTrio l, UIntTrio r) { return new UIntTrio(l.x - r.x, l.y - r.y, l.z - r.z); }
        public static UIntTrio operator *(IntTrio l, UIntTrio r) { return new UIntTrio(l.x * r.x, l.y * r.y, l.z * r.z); }
        public static UIntTrio operator /(IntTrio l, UIntTrio r) { return new UIntTrio(l.x / r.x, l.y / r.y, l.z / r.z); }

        public static UIntTrio operator +(UIntTrio l, IntTrio r) { return new UIntTrio(l.x + r.x, l.y + r.y, l.z + r.z); }
        public static UIntTrio operator -(UIntTrio l, IntTrio r) { return new UIntTrio(l.x - r.x, l.y - r.y, l.z - r.z); }
        public static UIntTrio operator *(UIntTrio l, IntTrio r) { return new UIntTrio(l.x * r.x, l.y * r.y, l.z * r.z); }
        public static UIntTrio operator /(UIntTrio l, IntTrio r) { return new UIntTrio(l.x / r.x, l.y / r.y, l.z / r.z); }

        public static float3 operator *(UIntTrio l, float3 r) { return float3(l.x * r.x, l.y * r.y, l.z * r.z); }
        public static float3 operator *(float3 l, UIntTrio r) { return float3(l.x * r.x, l.y * r.y, l.z * r.z); }

        public static float3 operator *(UIntTrio l, float r) { return float3(l.x * r, l.y * r, l.z * r); }
        public static float3 operator /(UIntTrio l, float r) { return float3(l.x / r, l.y / r, l.z / r); }

        public static float3 operator *(float l, UIntTrio r) { return float3(l * r.x, l * r.y, l * r.z); }
        public static float3 operator /(float l, UIntTrio r) { return float3(l / r.x, l / r.y, l / r.z); }

        public static implicit operator UIntTrio(IntTrio trio) { return new UIntTrio(trio.x, trio.y, trio.z); }
        public static implicit operator int3(UIntTrio trio) { return new int3(trio.x, trio.y, trio.z); }
        public static implicit operator UIntTrio(int3 i) { return new UIntTrio(i.x, i.y, i.z); }

    }

}