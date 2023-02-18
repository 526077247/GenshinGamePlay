using System;
using System.Collections.Generic;

namespace TaoTie
{
    public class AIMath
    {

        public const float EPSILON = 1E-05f; 
        public const float Infinity = 1f / 0f; 
        public const float NegativeInfinity = -1f / 0f; 
        public const float PI = 3.1415927f;
        public const float COS_DEG_80 = 0.173648f; 
        public const float COS_DEG_120 = -0.5f;
        public const float Deg2Rad = 0.017453292f; 
        public const float Rad2Deg = 57.29578f; 
        private static System.Random aiRand;
    }
}