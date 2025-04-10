using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

namespace Nebukam.Common
{
    public static class MathsExtensions
    {

        public static float Det(this Vector2 @this, Vector2 other) { return @this.x * other.y - @this.y * other.x; }
        public static float Det(this float2 @this, float2 other) { return @this.x * other.y - @this.y * other.x; }

        /// <summary>
        /// Lerp shorthand.
        /// </summary>
        /// <param name="this"></param>
        /// <param name="other"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Vector2 To(this Vector2 @this, Vector2 other, float t)
        {
            return new Vector2(@this.x + (other.x - @this.x) * t, @this.y + (other.y - @this.y) * t);
        }

        public static float2 To(this float2 @this, float2 other, float t)
        {
            return float2(@this.x + (other.x - @this.x) * t, @this.y + (other.y - @this.y) * t);
        }

        /// <summary>
        /// Lerp shorthand.
        /// </summary>
        /// <param name="this"></param>
        /// <param name="other"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Vector3 To(this Vector3 @this, Vector3 other, float t)
        {
            return new Vector3(@this.x + (other.x - @this.x) * t, @this.y + (other.y - @this.y) * t, @this.z + (other.z - @this.z) * t);
        }

        public static float3 To(this float3 @this, float3 other, float t)
        {
            return float3(@this.x + (other.x - @this.x) * t, @this.y + (other.y - @this.y) * t, @this.z + (other.z - @this.z) * t);
        }

        /// <summary>
        /// Is a point between a and b?
        /// </summary>
        /// <param name="this"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool IsBetween(this Vector2 @this, Vector2 a, Vector2 b)
        {

            Vector2 ab = new Vector2(b.x - a.x, b.y - a.y);//Entire line segment
            Vector2 ac = new Vector2(@this.x - a.x, @this.y - a.y);//The intersection and the first point

            float dot = ab.x * ac.x + ab.y * ac.y;

            //If the vectors are pointing in the same direction = dot product is positive
            if (dot <= 0f) { return false; }

            float abm = ab.x * ab.x + ab.y * ab.y;
            float acm = ac.x * ac.x + ac.y * ac.y;

            //If the length of the vector between the intersection and the first point is smaller than the entire line
            return (abm >= acm);
        }

        public static bool IsBetween(this float2 @this, float2 a, float2 b)
        {

            float2 ab = float2(b.x - a.x, b.y - a.y);//Entire line segment
            float2 ac = float2(@this.x - a.x, @this.y - a.y);//The intersection and the first point

            float dot = ab.x * ac.x + ab.y * ac.y;

            //If the vectors are pointing in the same direction = dot product is positive
            if (dot <= 0f) { return false; }

            float abm = ab.x * ab.x + ab.y * ab.y;
            float acm = ac.x * ac.x + ac.y * ac.y;

            //If the length of the vector between the intersection and the first point is smaller than the entire line
            return (abm >= acm);
        }

        /// <summary>
        /// Is a point between a and b?
        /// </summary>
        /// <param name="this"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool IsBetween(this Vector3 @this, Vector3 a, Vector3 b)
        {

            Vector3 ab = new Vector3(b.x - a.x, b.y - a.y, b.z - a.z); //Entire line segment
            Vector3 ac = new Vector3(@this.x - a.x, @this.y - a.y, @this.z - a.z);//The intersection and the first point

            float dot = ab.x * ac.x + ab.y * ac.y + ab.z * ac.z;

            //If the vectors are pointing in the same direction = dot product is positive
            if (dot <= 0f) { return false; }

            float abm = ab.x * ab.x + ab.y * ab.y + ab.z * ab.z;
            float acm = ac.x * ac.x + ac.y * ac.y + ac.z * ac.z;

            //If the length of the vector between the intersection and the first point is smaller than the entire line
            return (abm >= acm);
        }

        public static bool IsBetween(this float3 @this, float3 a, float3 b)
        {

            float3 ab = new float3(b.x - a.x, b.y - a.y, b.z - a.z); //Entire line segment
            float3 ac = new float3(@this.x - a.x, @this.y - a.y, @this.z - a.z);//The intersection and the first point

            float dot = ab.x * ac.x + ab.y * ac.y + ab.z * ac.z;

            //If the vectors are pointing in the same direction = dot product is positive
            if (dot <= 0f) { return false; }

            float abm = ab.x * ab.x + ab.y * ab.y + ab.z * ab.z;
            float acm = ac.x * ac.x + ac.y * ac.y + ac.z * ac.z;

            //If the length of the vector between the intersection and the first point is smaller than the entire line
            return (abm >= acm);
        }

        /// <summary>
        /// Checks whether this vector is orthogonal with another given vector
        /// </summary>
        /// <param name="this"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool IsOrthogonalTo(this Vector2 @this, Vector2 b)
        {
            //2 vectors are orthogonal is the dot product is 0
            //We have to check if close to 0 because of floating numbers
            float dot = @this.x * b.x + @this.y * b.y;
            return (dot < Maths.EPSILON && dot > Maths.NEPSILON);
        }

        public static bool IsOrthogonalTo(this float2 @this, float2 b)
        {
            //2 vectors are orthogonal is the dot product is 0
            //We have to check if close to 0 because of floating numbers
            float dot = @this.x * b.x + @this.y * b.y;
            return (dot < Maths.EPSILON && dot > Maths.NEPSILON);
        }

        /// <summary>
        /// Checks whether this vector is orthogonal with another given vector
        /// </summary>
        /// <param name="this"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool IsOrthogonalTo(this Vector3 @this, Vector3 b)
        {
            //2 vectors are orthogonal is the dot product is 0
            //We have to check if close to 0 because of floating numbers
            float dot = @this.x * b.x + @this.y * b.y + @this.z * b.z;
            return (dot < Maths.EPSILON && dot > Maths.NEPSILON);
        }

        public static bool IsOrthogonalTo(this float3 @this, float3 b)
        {
            //2 vectors are orthogonal is the dot product is 0
            //We have to check if close to 0 because of floating numbers
            float dot = @this.x * b.x + @this.y * b.y + @this.z * b.z;
            return (dot < Maths.EPSILON && dot > Maths.NEPSILON);
        }

        /// <summary>
        /// Checks whether this vector is parallel to another given vector
        /// </summary>
        /// <param name="this"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool IsParallelTo(this Vector2 @this, Vector2 b)
        {
            //2 vectors are parallel if the angle between the vectors are 0 or 180 degrees
            Vector3 an = @this.normalized, bn = b.normalized;
            float angle = Mathf.Acos(Mathf.Clamp((an.x * bn.x + an.y * bn.y), -1f, 1f)) * 57.29578f;
            return (angle == 0f || angle == 180f);
        }

        public static bool IsParallelTo(this float2 @this, float2 b)
        {
            //2 vectors are parallel if the angle between the vectors are 0 or 180 degrees
            float2 an = normalize(@this), bn = normalize(b);
            float angle = Mathf.Acos(Mathf.Clamp((an.x * bn.x + an.y * bn.y), -1f, 1f)) * 57.29578f;
            return (angle == 0f || angle == 180f);
        }

        /// <summary>
        /// Checks whether this vector is parallel to another given vector
        /// </summary>
        /// <param name="this"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool IsParallelTo(this Vector3 @this, Vector3 b)
        {
            //2 vectors are parallel if the angle between the vectors are 0 or 180 degrees
            Vector3 an = @this.normalized, bn = b.normalized;
            float angle = Mathf.Acos(Mathf.Clamp((an.x * bn.x + an.y * bn.y + an.z * bn.z), -1f, 1f)) * 57.29578f;
            return (angle == 0f || angle == 180f);
        }


        public static bool IsParallelTo(this float3 @this, float3 b)
        {
            //2 vectors are parallel if the angle between the vectors are 0 or 180 degrees
            float3 an = normalize(@this), bn = normalize(b);
            float angle = Mathf.Acos(Mathf.Clamp((an.x * bn.x + an.y * bn.y + an.z * bn.z), -1f, 1f)) * 57.29578f;
            return (angle == 0f || angle == 180f);
        }

    }
}
