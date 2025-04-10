using UnityEngine;

namespace Nebukam.Common
{
    static public class Colors
    {

        /// <summary>
        /// Concat an RGBA Color object into a single float.
        /// </summary>
        /// <param name="col"></param>
        /// <returns></returns>
        static public float ConcatRGBA(Color col)
        {
            float m = 1000f;
            float A = col.a * 255, R = col.r * 255, G = col.g * 255, B = col.b * 255;
            return (R * m * m * m) + (G * m * m) + (B * m) + A;

        }

        /// <summary>
        /// Concat an RGB Color object into a single float.
        /// </summary>
        /// <param name="col"></param>
        /// <returns></returns>
        static public float ConcatRGB(Color col)
        {
            float m = 1000f;
            return (col.r * m * m * m) + (col.g * m * m) + (col.b * m);

        }

        static public Color A(this Color col, float alpha)
        {
            col.a = alpha;
            return col;
        }


    }

}
