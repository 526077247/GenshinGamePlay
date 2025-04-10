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
    public struct UIntPair : System.IEquatable<UIntPair>
    {

        public static UIntPair zero = new UIntPair(0, 0);
        public int x, y;
        public byte d;

        public UIntPair(int x, int y)
        {
            d = 0;
            this.x = x;
            this.y = y;
        }

        public UIntPair(int x) : this(x, x) { }
        
        public UIntPair ascending { get { return x > y ? new UIntPair(y, x) : this; } }

        public UIntPair descending { get { return x < y ? new UIntPair(y, x) : this; } }

        public bool Contains(int i)
        {
            return (x == i || y == i);
        }

        public static bool operator !=(UIntPair e1, UIntPair e2)
        {
            return !((e1.x == e2.x && e1.y == e2.y) || (e1.x == e2.y && e1.y == e2.x));
        }

        public static bool operator ==(UIntPair e1, UIntPair e2)
        {
            return (e1.x == e2.x && e1.y == e2.y) || (e1.x == e2.y && e1.y == e2.x);
        }

        public bool Equals(UIntPair e)
        {
            return (e.x == x && e.y == y) || (e.x == y && e.y == x);
        }

        public override bool Equals(object obj)
        {
            UIntPair e = (UIntPair)obj;
            return (e.x == x && e.y == y) || (e.x == y && e.y == x);
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                return (x > y) ? x * 100000 + y : y * 100000 + x;
            }
        }

        public override string ToString()
        {
            return string.Format("({0},{1})", x, y);
        }

        public static UIntPair operator +(UIntPair l, int r) { return new UIntPair(l.x + r, l.y + r); }
        public static UIntPair operator -(UIntPair l, int r) { return new UIntPair(l.x - r, l.y - r); }
        public static UIntPair operator *(UIntPair l, int r) { return new UIntPair(l.x * r, l.y * r); }
        public static UIntPair operator /(UIntPair l, int r) { return new UIntPair(l.x / r, l.y / r); }

        public static UIntPair operator +(int l, UIntPair r) { return new UIntPair(r.x + l, r.y + l); }
        public static UIntPair operator -(int l, UIntPair r) { return new UIntPair(r.x - l, r.y - l); }
        public static UIntPair operator *(int l, UIntPair r) { return new UIntPair(r.x * l, r.y * l); }
        public static UIntPair operator /(int l, UIntPair r) { return new UIntPair(r.x / l, r.y / l); }

        public static UIntPair operator +(UIntPair l, UIntPair r) { return new UIntPair(l.x + r.x, l.y + r.y); }
        public static UIntPair operator -(UIntPair l, UIntPair r) { return new UIntPair(l.x - r.x, l.y - r.y); }
        public static UIntPair operator *(UIntPair l, UIntPair r) { return new UIntPair(l.x * r.x, l.y * r.y); }
        public static UIntPair operator /(UIntPair l, UIntPair r) { return new UIntPair(l.x / r.x, l.y / r.y); }

        public static UIntPair operator +(UIntPair l, int2 r) { return new UIntPair(l.x + r.x, l.y + r.y); }
        public static UIntPair operator -(UIntPair l, int2 r) { return new UIntPair(l.x - r.x, l.y - r.y); }
        public static UIntPair operator *(UIntPair l, int2 r) { return new UIntPair(l.x * r.x, l.y * r.y); }
        public static UIntPair operator /(UIntPair l, int2 r) { return new UIntPair(l.x / r.x, l.y / r.y); }

        public static UIntPair operator +(int2 l, UIntPair r) { return new UIntPair(l.x + r.x, l.y + r.y); }
        public static UIntPair operator -(int2 l, UIntPair r) { return new UIntPair(l.x - r.x, l.y - r.y); }
        public static UIntPair operator *(int2 l, UIntPair r) { return new UIntPair(l.x * r.x, l.y * r.y); }
        public static UIntPair operator /(int2 l, UIntPair r) { return new UIntPair(l.x / r.x, l.y / r.y); }

        public static UIntPair operator +(UIntPair l, IntPair r) { return new UIntPair(l.x + r.x, l.y + r.y); }
        public static UIntPair operator -(UIntPair l, IntPair r) { return new UIntPair(l.x - r.x, l.y - r.y); }
        public static UIntPair operator *(UIntPair l, IntPair r) { return new UIntPair(l.x * r.x, l.y * r.y); }
        public static UIntPair operator /(UIntPair l, IntPair r) { return new UIntPair(l.x / r.x, l.y / r.y); }

        public static UIntPair operator +(IntPair l, UIntPair r) { return new UIntPair(l.x + r.x, l.y + r.y); }
        public static UIntPair operator -(IntPair l, UIntPair r) { return new UIntPair(l.x - r.x, l.y - r.y); }
        public static UIntPair operator *(IntPair l, UIntPair r) { return new UIntPair(l.x * r.x, l.y * r.y); }
        public static UIntPair operator /(IntPair l, UIntPair r) { return new UIntPair(l.x / r.x, l.y / r.y); }

        public static float2 operator *(UIntPair l, float2 r) { return float2(l.x * r.x, l.y * r.y); }
        public static float2 operator *(float2 l, UIntPair r) { return float2(l.x * r.x, l.y * r.y); }

        public static float3 operator *(UIntPair l, float3 r) { return float3(l.x * r.x, l.y * r.y, r.z); }
        public static float3 operator *(float3 l, UIntPair r) { return float3(l.x * r.x, l.y * r.y, l.z); }

        public static float2 operator *(UIntPair l, float r) { return float2(l.x * r, l.y * r); }
        public static float2 operator /(UIntPair l, float r) { return float2(l.x / r, l.y / r); }

        public static float2 operator *(float l, UIntPair r) { return float2(l * r.x, l * r.y); }
        public static float2 operator /(float l, UIntPair r) { return float2(l / r.x, l / r.y); }

        public static implicit operator IntPair(UIntPair pair) { return new IntPair(pair.x, pair.y); }
        public static implicit operator int2(UIntPair pair) { return new int2(pair.x, pair.y); }
        public static implicit operator UIntPair(int2 i) { return new UIntPair(i.x, i.y); }

    }

}