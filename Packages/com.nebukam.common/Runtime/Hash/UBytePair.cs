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
    /// Individual int values must range between -100000 & 100000. Collision occur with higher values.
    /// </summary>
    [BurstCompile]
    public struct UBytePair : System.IEquatable<UBytePair>
    {

        public static UBytePair zero = new UBytePair(0, 0);
        public byte x, y;

        public UBytePair(byte x, byte y)
        {
            this.x = x;
            this.y = y;
        }

        public UBytePair(int x, int y)
        {
            this.x = (byte)x;
            this.y = (byte)y;
        }

        public UBytePair(byte x) : this(x, x) { }
        public UBytePair(int x) : this((byte)x, (byte)x) { }

        public UBytePair ascending { get { return x > y ? new UBytePair(y, x) : this; } }

        public UBytePair descending { get { return x < y ? new UBytePair(y, x) : this; } }

        public bool Contains(int i)
        {
            return (x == i || y == i);
        }

        public static bool operator !=(UBytePair e1, UBytePair e2)
        {
            return !((e1.x == e2.x && e1.y == e2.y) || (e1.x == e2.y && e1.y == e2.x));
        }

        public static bool operator ==(UBytePair e1, UBytePair e2)
        {
            return ((e1.x == e2.x && e1.y == e2.y)|| (e1.x == e2.y && e1.y == e2.x));
        }

        public bool Equals(UBytePair e)
        {
            return ((e.x == x && e.y == y)|| (e.x == y && e.y == x));
        }

        public override bool Equals(object obj)
        {
            UBytePair e = (UBytePair)obj;
            return ((e.x == x && e.y == y)|| (e.x == y && e.y == x));
        }

        public override int GetHashCode()
        {
            return (x > y) ? (x << 0) | (y << 8) : (y << 0) | (x << 8);
        }

        public override string ToString()
        {
            return string.Format("({0},{1})", x, y);
        }

        public static UBytePair operator +(UBytePair l, int r) { return new UBytePair(l.x + r, l.y + r); }
        public static UBytePair operator -(UBytePair l, int r) { return new UBytePair(l.x - r, l.y - r); }
        public static UBytePair operator *(UBytePair l, int r) { return new UBytePair(l.x * r, l.y * r); }
        public static UBytePair operator /(UBytePair l, int r) { return new UBytePair(l.x / r, l.y / r); }

        public static UBytePair operator +(int l, UBytePair r) { return new UBytePair(r.x + l, r.y + l); }
        public static UBytePair operator -(int l, UBytePair r) { return new UBytePair(r.x - l, r.y - l); }
        public static UBytePair operator *(int l, UBytePair r) { return new UBytePair(r.x * l, r.y * l); }
        public static UBytePair operator /(int l, UBytePair r) { return new UBytePair(r.x / l, r.y / l); }

        public static UBytePair operator +(UBytePair l, UBytePair r) { return new UBytePair(l.x + r.x, l.y + r.y); }
        public static UBytePair operator -(UBytePair l, UBytePair r) { return new UBytePair(l.x - r.x, l.y - r.y); }
        public static UBytePair operator *(UBytePair l, UBytePair r) { return new UBytePair(l.x * r.x, l.y * r.y); }
        public static UBytePair operator /(UBytePair l, UBytePair r) { return new UBytePair(l.x / r.x, l.y / r.y); }

        public static UBytePair operator +(UBytePair l, int2 r) { return new UBytePair(l.x + r.x, l.y + r.y); }
        public static UBytePair operator -(UBytePair l, int2 r) { return new UBytePair(l.x - r.x, l.y - r.y); }
        public static UBytePair operator *(UBytePair l, int2 r) { return new UBytePair(l.x * r.x, l.y * r.y); }
        public static UBytePair operator /(UBytePair l, int2 r) { return new UBytePair(l.x / r.x, l.y / r.y); }

        public static UBytePair operator +(int2 l, UBytePair r) { return new UBytePair(l.x + r.x, l.y + r.y); }
        public static UBytePair operator -(int2 l, UBytePair r) { return new UBytePair(l.x - r.x, l.y - r.y); }
        public static UBytePair operator *(int2 l, UBytePair r) { return new UBytePair(l.x * r.x, l.y * r.y); }
        public static UBytePair operator /(int2 l, UBytePair r) { return new UBytePair(l.x / r.x, l.y / r.y); }

        public static UBytePair operator +(UBytePair l, BytePair r) { return new UBytePair(l.x + r.x, l.y + r.y); }
        public static UBytePair operator -(UBytePair l, BytePair r) { return new UBytePair(l.x - r.x, l.y - r.y); }
        public static UBytePair operator *(UBytePair l, BytePair r) { return new UBytePair(l.x * r.x, l.y * r.y); }
        public static UBytePair operator /(UBytePair l, BytePair r) { return new UBytePair(l.x / r.x, l.y / r.y); }

        public static UBytePair operator +(BytePair l, UBytePair r) { return new UBytePair(l.x + r.x, l.y + r.y); }
        public static UBytePair operator -(BytePair l, UBytePair r) { return new UBytePair(l.x - r.x, l.y - r.y); }
        public static UBytePair operator *(BytePair l, UBytePair r) { return new UBytePair(l.x * r.x, l.y * r.y); }
        public static UBytePair operator /(BytePair l, UBytePair r) { return new UBytePair(l.x / r.x, l.y / r.y); }

        public static float2 operator *(UBytePair l, float2 r) { return float2(l.x * r.x, l.y * r.y); }
        public static float2 operator *(float2 l, UBytePair r) { return float2(l.x * r.x, l.y * r.y); }

        public static float3 operator *(UBytePair l, float3 r) { return float3(l.x * r.x, l.y * r.y, r.z); }
        public static float3 operator *(float3 l, UBytePair r) { return float3(l.x * r.x, l.y * r.y, l.z); }

        public static float2 operator *(UBytePair l, float r) { return float2(l.x * r, l.y * r); }
        public static float2 operator /(UBytePair l, float r) { return float2(l.x / r, l.y / r); }

        public static float2 operator *(float l, UBytePair r) { return float2(l * r.x, l * r.y); }
        public static float2 operator /(float l, UBytePair r) { return float2(l / r.x, l / r.y); }

        public static implicit operator BytePair(UBytePair pair) { return new BytePair(pair.x, pair.y); }
        public static implicit operator int2(UBytePair pair) { return int2(pair.x, pair.y); }
        public static implicit operator UBytePair(int2 i) { return int2(i.x, i.y); }

        public static explicit operator UBytePair(int i) { return new UBytePair((i >> 0) & 255, (i >> 8) & 255); }
        public static explicit operator int(UBytePair i) { return (i.x << 0) | (i.y << 8); }

    }

}