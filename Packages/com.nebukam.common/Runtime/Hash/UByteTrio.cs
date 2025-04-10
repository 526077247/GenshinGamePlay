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
    public struct UByteTrio : System.IEquatable<UByteTrio>
    {

        public static UByteTrio zero = new UByteTrio(0, 0, 0);
        public byte x, y, z, d;

        public UByteTrio(int x, int y, int z)
        {
            d = 0;
            this.x = (byte)x;
            this.y = (byte)y;
            this.z = (byte)z;
        }

        public UByteTrio(byte x, byte y, byte z)
        {
            d = 0;
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public UByteTrio(int x) : this(x, x, x) { }
        public UByteTrio(int x, int y) : this(x, y, 0) { }

        public UByteTrio(byte x) : this(x, x, x) { }
        public UByteTrio(byte x, byte y) : this(x, y, (byte)0) { }

        public bool Contains(int i)
        {
            return (x == i || y == i || z == i);
        }

        public static bool operator !=(UByteTrio e1, UByteTrio e2)
        {
            return !(e1 == e2);
        }

        public static bool operator ==(UByteTrio e1, UByteTrio e2)
        {
            return ((e1.x == e2.x && e1.y == e2.y && e1.z == e2.z)
                || (e1.x == e2.x && e1.y == e2.z && e1.z == e2.y)
                || (e1.x == e2.y && e1.y == e2.x && e1.z == e2.z)
                || (e1.x == e2.y && e1.y == e2.z && e1.z == e2.x)
                || (e1.x == e2.z && e1.y == e2.x && e1.z == e2.y)
                || (e1.x == e2.z && e1.y == e2.y && e1.z == e2.x));
        }

        public bool Equals(UByteTrio e)
        {
            return this == e;
        }

        public override bool Equals(object obj)
        {
            UByteTrio e = (UByteTrio)obj;
            return this == e;
        }

        public override int GetHashCode()
        {

            int A = max((int)x, (int)y); A = max(A, z);
            int B, C;
            if (A == x)
            {
                if (y > z) { B = y; C = z; }
                else { B = z; C = y; }
            }
            else if (A == y)
            {
                if (x > z) { B = x; C = z; }
                else { B = z; C = x; }
            }
            else
            {
                if (x > y) { B = x; C = z; }
                else { B = z; C = x; }
            }

            return (A << 0) | (B << 8) | (C << 16);

        }

        public override string ToString()
        {
            return string.Format("({0},{1},{2})", x, y, z);
        }

        public static UByteTrio operator +(UByteTrio l, int r) { return new UByteTrio(l.x + r, l.y + r, l.z + r); }
        public static UByteTrio operator -(UByteTrio l, int r) { return new UByteTrio(l.x - r, l.y - r, l.z + r); }
        public static UByteTrio operator *(UByteTrio l, int r) { return new UByteTrio(l.x * r, l.y * r, l.z * r); }
        public static UByteTrio operator /(UByteTrio l, int r) { return new UByteTrio(l.x / r, l.y / r, l.z / r); }

        public static UByteTrio operator +(int l, UByteTrio r) { return new UByteTrio(r.x + l, r.y + l, r.z + l); }
        public static UByteTrio operator -(int l, UByteTrio r) { return new UByteTrio(r.x - l, r.y - l, r.z + l); }
        public static UByteTrio operator *(int l, UByteTrio r) { return new UByteTrio(r.x * l, r.y * l, r.z * l); }
        public static UByteTrio operator /(int l, UByteTrio r) { return new UByteTrio(r.x / l, r.y / l, r.z / l); }

        public static UByteTrio operator +(UByteTrio l, UByteTrio r) { return new UByteTrio(l.x + r.x, l.y + r.y, l.z + r.z); }
        public static UByteTrio operator -(UByteTrio l, UByteTrio r) { return new UByteTrio(l.x - r.x, l.y - r.y, l.z - r.z); }
        public static UByteTrio operator *(UByteTrio l, UByteTrio r) { return new UByteTrio(l.x * r.x, l.y * r.y, l.z * r.z); }
        public static UByteTrio operator /(UByteTrio l, UByteTrio r) { return new UByteTrio(l.x / r.x, l.y / r.y, l.z / r.z); }

        public static UByteTrio operator +(UByteTrio l, int3 r) { return new UByteTrio(l.x + r.x, l.y + r.y, l.z + r.z); }
        public static UByteTrio operator -(UByteTrio l, int3 r) { return new UByteTrio(l.x - r.x, l.y - r.y, l.z - r.z); }
        public static UByteTrio operator *(UByteTrio l, int3 r) { return new UByteTrio(l.x * r.x, l.y * r.y, l.z * r.z); }
        public static UByteTrio operator /(UByteTrio l, int3 r) { return new UByteTrio(l.x / r.x, l.y / r.y, l.z / r.z); }

        public static UByteTrio operator +(int3 l, UByteTrio r) { return new UByteTrio(l.x + r.x, l.y + r.y, l.z + r.z); }
        public static UByteTrio operator -(int3 l, UByteTrio r) { return new UByteTrio(l.x - r.x, l.y - r.y, l.z - r.z); }
        public static UByteTrio operator *(int3 l, UByteTrio r) { return new UByteTrio(l.x * r.x, l.y * r.y, l.z * r.z); }
        public static UByteTrio operator /(int3 l, UByteTrio r) { return new UByteTrio(l.x / r.x, l.y / r.y, l.z / r.z); }

        public static UByteTrio operator +(IntTrio l, UByteTrio r) { return new UByteTrio(l.x + r.x, l.y + r.y, l.z + r.z); }
        public static UByteTrio operator -(IntTrio l, UByteTrio r) { return new UByteTrio(l.x - r.x, l.y - r.y, l.z - r.z); }
        public static UByteTrio operator *(IntTrio l, UByteTrio r) { return new UByteTrio(l.x * r.x, l.y * r.y, l.z * r.z); }
        public static UByteTrio operator /(IntTrio l, UByteTrio r) { return new UByteTrio(l.x / r.x, l.y / r.y, l.z / r.z); }

        public static UByteTrio operator +(UByteTrio l, IntTrio r) { return new UByteTrio(l.x + r.x, l.y + r.y, l.z + r.z); }
        public static UByteTrio operator -(UByteTrio l, IntTrio r) { return new UByteTrio(l.x - r.x, l.y - r.y, l.z - r.z); }
        public static UByteTrio operator *(UByteTrio l, IntTrio r) { return new UByteTrio(l.x * r.x, l.y * r.y, l.z * r.z); }
        public static UByteTrio operator /(UByteTrio l, IntTrio r) { return new UByteTrio(l.x / r.x, l.y / r.y, l.z / r.z); }

        public static float3 operator *(UByteTrio l, float3 r) { return float3(l.x * r.x, l.y * r.y, l.z * r.z); }
        public static float3 operator *(float3 l, UByteTrio r) { return float3(l.x * r.x, l.y * r.y, l.z * r.z); }

        public static float3 operator *(UByteTrio l, float r) { return float3(l.x * r, l.y * r, l.z * r); }
        public static float3 operator /(UByteTrio l, float r) { return float3(l.x / r, l.y / r, l.z / r); }

        public static float3 operator *(float l, UByteTrio r) { return float3(l * r.x, l * r.y, l * r.z); }
        public static float3 operator /(float l, UByteTrio r) { return float3(l / r.x, l / r.y, l / r.z); }

        public static implicit operator UByteTrio(IntTrio trio) { return new UByteTrio(trio.x, trio.y, trio.z); }
        public static implicit operator int3(UByteTrio trio) { return new int3(trio.x, trio.y, trio.z); }
        public static implicit operator UByteTrio(int3 i) { return new UByteTrio(i.x, i.y, i.z); }

        public static explicit operator UByteTrio(int i) { return new UByteTrio((i >> 0) & 255, (i >> 8) & 255, (i >> 16) & 255); }
        public static explicit operator int(UByteTrio i) { return (i.x << 0) | (i.y << 8) | (i.z << 16); }

    }

}