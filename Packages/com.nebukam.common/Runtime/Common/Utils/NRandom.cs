using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;
using Unity.Collections;
using Unity.Burst;

namespace Nebukam.Common
{
    static public class NRandom
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="weights"></param>
        /// <returns></returns>
        public static int GetRandomWeightedIndex(float[] weights)
        {
            if (weights == null || weights.Length == 0) return -1;

            float w;
            float t = 0;
            int i;
            for (i = 0; i < weights.Length; i++)
            {
                w = weights[i];
                if (float.IsPositiveInfinity(w)) return i;
                else if (w >= 0f && !float.IsNaN(w)) t += weights[i];
            }

            float r = UnityEngine.Random.value;
            float s = 0f;

            for (i = 0; i < weights.Length; i++)
            {
                w = weights[i];
                if (float.IsNaN(w) || w <= 0f) continue;

                s += w / t;
                if (s >= r) return i;
            }

            return -1;
        }

        /// <summary>
        /// Job-friendly version
        /// </summary>
        /// <param name="weights"></param>
        /// <param name="randomValue"></param>
        /// <returns></returns>
        [BurstCompile]
        public static int GetRandomWeightedIndex(ref NativeList<float> weights, float randomValue)
        {
            if (weights.Length == 0) return -1;

            float w;
            float t = 0;
            int i;
            for (i = 0; i < weights.Length; i++)
            {
                w = weights[i];
                if (float.IsPositiveInfinity(w)) return i;
                else if (w >= 0f && !float.IsNaN(w)) t += weights[i];
            }

            float s = 0f;

            for (i = 0; i < weights.Length; i++)
            {
                w = weights[i];
                if (float.IsNaN(w) || w <= 0f) continue;

                s += w / t;
                if (s >= randomValue) return i;
            }

            return -1;
        }

    }
}