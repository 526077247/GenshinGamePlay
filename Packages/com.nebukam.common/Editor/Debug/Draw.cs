#if UNITY_EDITOR
using System;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

namespace Nebukam.Common.Editor
{
    static public class Draw
    {

        /// <summary>
        /// Draw a triangle
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <param name="C"></param>
        static public void Tri(float3 A, float3 B, float3 C)
        {
            Line(A, B); Line(B, C); Line(C, A);
        }

        /// <summary>
        /// Draw a triangle
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <param name="C"></param>
        static public void Tri(float3 A, float3 B, float3 C, Color col)
        {
            Line(A, B, col); Line(B, C, col); Line(C, A, col);
        }

        /// <summary>
        /// Draw a line between two positions.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        static public void Line(float3 from, float3 to)
        {
            Line(from, to, Color.red);
        }

        /// <summary>
        /// Draw a line between two positions, with a specific color.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="col"></param>
        static public void Line(float3 from, float3 to, Color col)
        {
            Debug.DrawLine(from, to, col);
        }

        /// <summary>
        /// Draw a circle (XZ plane).
        /// </summary>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        static public void Circle(float3 center, float radius, int samples = 30)
        {
            Circle(center, radius, Color.red, samples);
        }

        /// <summary>
        /// Draw a circle (XZ plane), with a specific color.
        /// </summary>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        /// <param name="col"></param>
        static public void Circle(float3 center, float radius, Color col, int samples = 30)
        {

            float3 from, to;

            float angleIncrease = (float)(Math.PI * 2) / samples;
            from = to = float3(center.x + radius * (float)Math.Cos(0.0f), center.y, center.z + radius * (float)Math.Sin(0.0f));

            for (int i = 0; i < samples; i++)
            {

                float rad = angleIncrease * (i + 1);

                to = float3(center.x + radius * Mathf.Cos(rad), center.y, center.z + radius * Mathf.Sin(rad));

                Line(from, to, col);

                from = to;


            }
        }

        /// <summary>
        /// Draw a circle (XY plane).
        /// </summary>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        static public void Circle2D(float3 center, float radius, int samples = 30)
        {
            Circle2D(center, radius, Color.red, samples);
        }

        /// <summary>
        /// Draw a circle (XY plane), with a specific color.
        /// </summary>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        /// <param name="col"></param>
        static public void Circle2D(float3 center, float radius, Color col, int samples = 30)
        {

            float3 from, to;

            float angleIncrease = (float)(Math.PI * 2) / samples;
            from = to = float3(center.x + radius * Mathf.Cos(0.0f), center.y + radius * Mathf.Sin(0.0f), center.z);

            for (int i = 0; i < samples; i++)
            {

                float rad = angleIncrease * (i + 1);

                to = float3(center.x + radius * Mathf.Cos(rad), center.y + radius * Mathf.Sin(rad), center.z);

                Line(from, to, col);

                from = to;


            }
        }

        /// <summary>
        /// Draw a square at the specified location (XZ plane).
        /// </summary>
        /// <param name="center"></param>
        /// <param name="size"></param>
        /// <param name="col"></param>
        static public void Square(float3 center, float size, Color col)
        {
            float s = size * 0.5f;

            float3 A = center + float3(-s, 0f, -s);
            float3 B = center + float3(-s, 0f, s);
            float3 C = center + float3(s, 0f, s);
            float3 D = center + float3(s, 0f, -s);

            Line(A, B, col);
            Line(B, C, col);
            Line(C, D, col);
            Line(D, A, col);

        }

        /// <summary>
        /// Draw a square at the specified location (XZ plane).
        /// </summary>
        /// <param name="center"></param>
        /// <param name="size"></param>
        /// <param name="col"></param>
        static public void Cube(float3 center, float size, Color col)
        {
            float s = size * 0.5f;

            float3 A = center + float3(-s, s, -s);
            float3 B = center + float3(-s, s, s);
            float3 C = center + float3(s, s, s);
            float3 D = center + float3(s, s, -s);

            float3 E = center + float3(-s, -s, -s);
            float3 F = center + float3(-s, -s, s);
            float3 G = center + float3(s, -s, s);
            float3 H = center + float3(s, -s, -s);

            Line(A, B, col);
            Line(B, C, col);
            Line(C, D, col);
            Line(D, A, col);

            Line(E, F, col);
            Line(F, G, col);
            Line(G, H, col);
            Line(H, E, col);

            Line(A, E, col);
            Line(B, F, col);
            Line(C, G, col);
            Line(D, H, col);

        }

    }
}
#endif