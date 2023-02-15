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

        public struct AICurve
        {
            public List<AIPoint> data;

            public AICurve(AIPoint[] inputData)
            {
                data = new List<AIPoint>();
                data.AddRange(inputData);
                SortByX();
            }

            public bool FindY(float xVal, ref float yVal)
            {
                for (int i = 0; i < data.Count; i++)
                {
                    if (data[i].x > xVal)
                    {
                        yVal = data[i].y;
                        return true;
                    }
                }
                return false;
            }

            public void SortByX()
            {
                AIPoint temp;
                for (int i = 0; i < data.Count; i++)
                {
                    for (int j = 0; j < data.Count - 1; j++)
                    {
                        if (data[i].x < data[j].x)
                        {
                            temp = data[i];
                            data[i] = data[j];
                            data[j] = temp;
                        }
                    }
                }
            }
        }

        [Serializable]
        public class AIPoint
        {
            private float xRawNum;
            private float yRawNum;
            public float x => xRawNum;
            public float y => yRawNum;
        }
    }
}