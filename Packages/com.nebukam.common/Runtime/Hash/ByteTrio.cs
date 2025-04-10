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
    public struct ByteTrio : System.IEquatable<ByteTrio>
    {

        public static ByteTrio zero = new ByteTrio(0, 0, 0);
        public byte x, y, z, d;
        
        public ByteTrio(int x, int y, int z)
        {
            d = 0;
            this.x = (byte)x;
            this.y = (byte)y;
            this.z = (byte)z;
        }

        public ByteTrio(byte x, byte y, byte z)
        {
            d = 0;
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public ByteTrio(int x) : this(x, x, x) { }
        public ByteTrio(int x, int y) : this(x, y, 0) { }

        public ByteTrio(byte x) : this(x, x, x) { }
        public ByteTrio(byte x, byte y) : this(x, y, (byte)0) { }

        public bool Contains(int i)
        {
            return (x == i || y == i || z == i);
        }

        public static bool operator !=(ByteTrio e1, ByteTrio e2)
        {
            return !(e1.x == e2.x && e1.y == e2.y && e1.z == e2.z);
        }

        public static bool operator ==(ByteTrio e1, ByteTrio e2)
        {
            return (e1.x == e2.x && e1.y == e2.y && e1.z == e2.z);
        }

        public bool Equals(ByteTrio e)
        {
            return this == e;
        }

        public override bool Equals(object obj)
        {
            ByteTrio e = (ByteTrio)obj;
            return (x == e.x && y == e.y && z == e.z);
        }

        public override int GetHashCode()
        {
            return (x << 0) | (y << 8) | (z << 16);
        }

        public override string ToString()
        {
            return string.Format("({0},{1},{2})", x, y, z);
        }

        public static ByteTrio operator +(ByteTrio l, int r) { return new ByteTrio(l.x + r, l.y + r, l.z + r); }
        public static ByteTrio operator -(ByteTrio l, int r) { return new ByteTrio(l.x - r, l.y - r, l.z + r); }
        public static ByteTrio operator *(ByteTrio l, int r) { return new ByteTrio(l.x * r, l.y * r, l.z * r); }
        public static ByteTrio operator /(ByteTrio l, int r) { return new ByteTrio(l.x / r, l.y / r, l.z / r); }

        public static ByteTrio operator +(int l, ByteTrio r) { return new ByteTrio(r.x + l, r.y + l, r.z + l); }
        public static ByteTrio operator -(int l, ByteTrio r) { return new ByteTrio(r.x - l, r.y - l, r.z + l); }
        public static ByteTrio operator *(int l, ByteTrio r) { return new ByteTrio(r.x * l, r.y * l, r.z * l); }
        public static ByteTrio operator /(int l, ByteTrio r) { return new ByteTrio(r.x / l, r.y / l, r.z / l); }

        public static ByteTrio operator +(ByteTrio l, ByteTrio r) { return new ByteTrio(l.x + r.x, l.y + r.y, l.z + r.z); }
        public static ByteTrio operator -(ByteTrio l, ByteTrio r) { return new ByteTrio(l.x - r.x, l.y - r.y, l.z - r.z); }
        public static ByteTrio operator *(ByteTrio l, ByteTrio r) { return new ByteTrio(l.x * r.x, l.y * r.y, l.z * r.z); }
        public static ByteTrio operator /(ByteTrio l, ByteTrio r) { return new ByteTrio(l.x / r.x, l.y / r.y, l.z / r.z); }

        public static ByteTrio operator +(ByteTrio l, int3 r) { return new ByteTrio(l.x + r.x, l.y + r.y, l.z + r.z); }
        public static ByteTrio operator -(ByteTrio l, int3 r) { return new ByteTrio(l.x - r.x, l.y - r.y, l.z - r.z); }
        public static ByteTrio operator *(ByteTrio l, int3 r) { return new ByteTrio(l.x * r.x, l.y * r.y, l.z * r.z); }
        public static ByteTrio operator /(ByteTrio l, int3 r) { return new ByteTrio(l.x / r.x, l.y / r.y, l.z / r.z); }

        public static ByteTrio operator +(int3 l, ByteTrio r) { return new ByteTrio(l.x + r.x, l.y + r.y, l.z + r.z); }
        public static ByteTrio operator -(int3 l, ByteTrio r) { return new ByteTrio(l.x - r.x, l.y - r.y, l.z - r.z); }
        public static ByteTrio operator *(int3 l, ByteTrio r) { return new ByteTrio(l.x * r.x, l.y * r.y, l.z * r.z); }
        public static ByteTrio operator /(int3 l, ByteTrio r) { return new ByteTrio(l.x / r.x, l.y / r.y, l.z / r.z); }

        public static ByteTrio operator +(ByteTrio l, UByteTrio r) { return new ByteTrio(l.x + r.x, l.y + r.y, l.z + r.z); }
        public static ByteTrio operator -(ByteTrio l, UByteTrio r) { return new ByteTrio(l.x - r.x, l.y - r.y, l.z - r.z); }
        public static ByteTrio operator *(ByteTrio l, UByteTrio r) { return new ByteTrio(l.x * r.x, l.y * r.y, l.z * r.z); }
        public static ByteTrio operator /(ByteTrio l, UByteTrio r) { return new ByteTrio(l.x / r.x, l.y / r.y, l.z / r.z); }

        public static ByteTrio operator +(UByteTrio l, ByteTrio r) { return new ByteTrio(l.x + r.x, l.y + r.y, l.z + r.z); }
        public static ByteTrio operator -(UByteTrio l, ByteTrio r) { return new ByteTrio(l.x - r.x, l.y - r.y, l.z - r.z); }
        public static ByteTrio operator *(UByteTrio l, ByteTrio r) { return new ByteTrio(l.x * r.x, l.y * r.y, l.z * r.z); }
        public static ByteTrio operator /(UByteTrio l, ByteTrio r) { return new ByteTrio(l.x / r.x, l.y / r.y, l.z / r.z); }

        public static float3 operator *(ByteTrio l, float3 r) { return float3(l.x * r.x, l.y * r.y, l.z * r.z); }
        public static float3 operator *(float3 l, ByteTrio r) { return float3(l.x * r.x, l.y * r.y, l.z * r.z); }

        public static float3 operator *(ByteTrio l, float r) { return float3(l.x * r, l.y * r, l.z * r); }
        public static float3 operator /(ByteTrio l, float r) { return float3(l.x / r, l.y / r, l.z / r); }

        public static float3 operator *(float l, ByteTrio r) { return float3(l * r.x, l * r.y, l * r.z); }
        public static float3 operator /(float l, ByteTrio r) { return float3(l / r.x, l / r.y, l / r.z); }

        public static implicit operator ByteTrio(UByteTrio trio) { return new ByteTrio(trio.x, trio.y, trio.z); }
        public static implicit operator int3(ByteTrio trio) { return new int3(trio.x, trio.y, trio.z); }
        public static implicit operator ByteTrio(int3 i) { return new ByteTrio(i.x, i.y, i.z); }

        public static explicit operator ByteTrio(int i) { return new ByteTrio((i >> 0) & 255, (i >> 8) & 255, (i >> 16) & 255); }
        public static explicit operator int(ByteTrio i) { return (i.x << 0) | (i.y << 8) | (i.z << 16); }
    }

}