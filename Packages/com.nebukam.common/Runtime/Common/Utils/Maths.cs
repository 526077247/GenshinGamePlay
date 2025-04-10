using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

namespace Nebukam.Common
{
    static public class Maths
    {

        #region Constants

        public const float EPSILON = 0.00001f;
        public const float NEPSILON = -0.00001f;
        public const float TAU = (float)Math.PI * 2.0f;

        #endregion

        #region Random

        /// <summary>
        /// A random value ranging from 0 to a given range. 
        /// Mirrored, the value ranges from -range to +range
        /// </summary>
        /// <param name="range"></param>
        /// <param name="mirror"></param>
        /// <returns></returns>
        public static float Rand(float range, bool mirror = true)
        {
            return UnityEngine.Random.value * range - (mirror ? UnityEngine.Random.value * range : 0.0f);
        }

        /// <summary>
        /// Return a random value within a given min/max range.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static float RandRange(float min, float max)
        {
            return min + Rand(max - min, false);
        }


        #endregion

        #region Scalar

        /// <summary>
        /// Normalized value remapping
        /// Example : remap 0.5 to (0.5, 1.0) = 0;
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static float NrmRemap(float val, float min, float max = 1.0f)
        {
            float diff = val - min;

            if (diff <= 0.0f)
            {
                return 0.0f;
            }

            float scale = max - min;

            return diff / scale;
        }

        /// <summary>
        /// Convert a value in radian into degree
        /// </summary>
        /// <param name="radian"></param>
        /// <returns></returns>
        public static float Degrees(float radian)
        {
            return radian * 57.29578f;
        }

        /// <summary>
        /// Return the angle of a 2D point, in degree from -180 to 180
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static float FindDegree(float x, float y)
        {
            float value = (atan2(x, y) / math.PI) * 180f;
            if (value < 0) value += 360f;

            return value;
        }

        /// <summary>
        /// Return a scale ratio so a given content size fits in a given container size.
        /// </summary>
        /// <param name="containerSize"></param>
        /// <param name="contentSize"></param>
        /// <returns></returns>
        public static float PreserveRatio(Vector2 containerSize, Vector2 contentSize)
        {

            float contentRatio = contentSize.x / contentSize.y;
            float containerRatio = containerSize.x / containerSize.y;

            return containerRatio > contentRatio ? containerSize.y / contentSize.y : containerSize.x / contentSize.x;

        }

        #endregion

        #region Vector

        #region normals

        /// <summary>
        /// Return the Normal vector of an A, B, C triad
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <param name="C"></param>
        /// <returns></returns>
        public static float3 Normal(float3 A, float3 B, float3 C)
        {
            return normalize(cross((B - A), (A - C)));
        }

        /// <summary>
        /// Normal | C = B + Vector3.up
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static float3 Perp(float3 A, float3 B)
        {
            return Normal(A, B, B + float3(0f, 1f, 0f));
        }

        /// <summary>
        /// Normal | C = B+dir
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static float3 NormalDir(float3 A, float3 B, float3 dir)
        {
            return Normal(A, B, B + dir);
        }

        #endregion

        /// <summary>
        /// Multiply each vector's component individually, returning the resulting vector.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 Mult(float3 a, float3 b) { return new float3(a.x * b.x, a.y * b.y, a.z * b.z); }

        #region rotations

        /// <summary>
        /// Rotate a point around a pivot, by an amount formatted as a Quaternion
        /// </summary>
        /// <param name="point"></param>
        /// <param name="pivot"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 RotateAroundPivot(float3 point, float3 pivot, quaternion angle)
        {
            return mul(angle, (point - pivot)) + pivot;
        }

        /// <summary>
        /// Rotate a point around a pivot, by a given amount on each axis.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="pivot"></param>
        /// <param name="angles">Radians</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 RotateAroundPivot(float3 point, float3 pivot, float3 angles)
        {
            float3 dir = point - pivot; // get point direction relative to pivot
            dir = mul(Unity.Mathematics.quaternion.EulerXYZ(angles), dir); // rotate it
            point = dir + pivot; // calculate rotated point
            return point; // return it
        }

        /// <summary>
        /// Rotate point around an axis and a given direction.
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="axis"></param>
        /// <param name="dir"></param>
        /// <param name="radius"></param>
        /// <param name="radAngle"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 RotatePointAroundAxisDir(float3 origin, float3 axis, float3 dir, float radius, float radAngle)
        {

            float3 a, b, c, n;

            a = float3(0f);
            b = axis;
            c = dir;

            n = normalize(cross(b - a, c - a));
            quaternion rot = Unity.Mathematics.quaternion.AxisAngle(axis, radAngle);
            //Quaternion rot = Quaternion.AngleAxis(radAngle / 0.0174532924f, axis); // get the desired rotation

            n = mul(rot, n);

            return origin + (n * radius);

        }

        #endregion


        /// <summary>
        /// Computes the determinant of a two-dimensional square matrix 
        /// with rows consisting of the specified two-dimensional vectors.
        /// </summary>
        /// <param name="a">The top row of the two-dimensional square matrix</param>
        /// <param name="b">The bottom row of the two-dimensional square matrix</param>
        /// <returns>The determinant of the two-dimensional square matrix.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Det(float2 a, float2 b)
        {
            return a.x * b.y - a.y * b.x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dot(float2 a, float2 b)
        {
            return a.x * b.x + a.y * b.y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dot(float3 a, float3 b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z;
        }

        /// <summary>
        /// Computes the squared length of a specified two-dimensional vector.
        /// </summary>
        /// <param name="v"></param>
        /// <returns>The squared length of the two-dimensional vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float AbsSq(float2 v)
        {
            return v.x * v.x + v.y * v.y;
        }

        /// <summary>
        /// Computes the squared length of a specified two-dimensional vector.
        /// </summary>
        /// <param name="v"></param>
        /// <returns>The squared length of the two-dimensional vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float AbsSq(float3 v)
        {
            return v.x * v.x + v.y * v.y + v.z * v.z;
        }

        /// <summary>
        /// >Computes the length of a specified two-dimensional vector.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Abs(float2 v)
        {
            return sqrt(v.x * v.x + v.y * v.y);
        }

        /// <summary>
        /// >Computes the length of a specified two-dimensional vector.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Abs(float3 v)
        {
            return sqrt(v.x * v.x + v.y * v.y + v.z * v.z);
        }

        /// <summary>
        /// Computes the signed distance from a line connecting the specified points to a specified point.
        /// </summary>
        /// <param name="a">The first point on the line.</param>
        /// <param name="b">The second point on the line.</param>
        /// <param name="c">The point to which the signed distance is to be calculated.</param>
        /// <returns>Positive when the point c lies to the left of the line ab.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float LeftOf(float2 a, float2 b, float2 c)
        {
            float x1 = a.x - c.x, y1 = a.y - c.y, x2 = b.x - a.x, y2 = b.y - a.y;
            return x1 * y2 - y1 * x2;
        }

        #endregion

        #region Line

        /// <summary>
        /// Computes the squared distance from a line segment with the specified endpoints to a specified point.
        /// </summary>
        /// <param name="a">The first endpoint of the line segment.</param>
        /// <param name="b">The second endpoint of the line segment.</param>
        /// <param name="c">The point to which the squared distance is to be calculated.</param>
        /// <returns>The squared distance from the line segment to the point.</returns>
        public static float DistSqPointLineSegment(float2 a, float2 b, float2 c)
        {

            //TODO : inline operations instead of calling shorthands
            float2 ca = float2(c.x - a.x, c.y - a.y);
            float2 ba = float2(b.x - a.x, b.y - a.y);
            float dot = ca.x * ba.x + ca.y * ba.y;

            float r = dot / (ba.x * ba.x + ba.y * ba.y);

            if (r < 0.0f)
            {
                return ca.x * ca.x + ca.y * ca.y;
            }

            if (r > 1.0f)
            {
                float2 cb = float2(c.x - b.x, c.y - b.y);
                return cb.x * cb.x + cb.y * cb.y;
            }

            float2 d = float2(c.x - (a.x + r * ba.x), c.y - (a.y + r * ba.y));
            return d.x * d.x + d.y * d.y;

        }

        /// <summary>
        /// Is a point c between a and b?
        /// </summary>
        /// <param name="c"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool IsBetween(float2 a, float2 b, float2 c)
        {

            float2 ab = float2(b.x - a.x, b.y - a.y);//Entire line segment
            float2 ac = float2(c.x - a.x, c.y - a.y);//The intersection and the first point

            float dot = ab.x * ac.x + ab.y * ac.y;

            //If the vectors are pointing in the same direction = dot product is positive
            if (dot <= 0f) { return false; }

            float abm = ab.x * ab.x + ab.y * ab.y;
            float acm = ac.x * ac.x + ac.y * ac.y;

            //If the length of the vector between the intersection and the first point is smaller than the entire line
            return (abm >= acm);
        }

        /// <summary>
        /// Is a point c between a and b?
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsBetween(float3 a, float3 b, float3 c)
        {

            float3 ab = float3(b.x - a.x, b.y - a.y, b.z - a.z);//Entire line segment
            float3 ac = float3(c.x - a.x, c.y - a.y, c.z - a.z);//The intersection and the first point

            float dot = ab.x * ac.x + ab.y * ac.y + ab.z * ac.z;

            //If the vectors are pointing in the same direction = dot product is positive
            if (dot <= 0f) { return false; }

            float abm = ab.x * ab.x + ab.y * ab.y + ab.z * ab.z;
            float acm = ac.x * ac.x + ac.y * ac.y + ac.z * ac.z;

            //If the length of the vector between the intersection and the first point is smaller than the entire line
            return (abm >= acm);
        }

        /// <summary>
        /// Checks whether two vector are orthogonal
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool IsOrthogonal(float2 a, float2 b)
        {
            //2 vectors are orthogonal is the dot product is 0
            //We have to check if close to 0 because of floating numbers
            float dot = a.x * b.x + a.y * b.y;
            return (dot < EPSILON && dot > NEPSILON);
        }

        /// <summary>
        /// Checks whether two vector are orthogonal
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool IsOrthogonal(float3 a, float3 b)
        {
            //2 vectors are orthogonal is the dot product is 0
            //We have to check if close to 0 because of floating numbers
            float dot = a.x * b.x + a.y * b.y + a.z * b.z;
            return (dot < EPSILON && dot > NEPSILON);
        }

        /// <summary>
        /// Checks whether two vector are parallel or not
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool IsParallel(float2 a, float2 b)
        {
            //2 vectors are parallel if the angle between the vectors are 0 or 180 degrees
            float2 an = normalize(a), bn = normalize(b);
            float angle = acos(clamp((an.x * bn.x + an.y * bn.y), -1f, 1f)) * 57.29578f;
            return (angle == 0f || angle == 180f);
        }

        /// <summary>
        /// Checks whether two vector are parallel or not
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool IsParallel(float3 a, float3 b)
        {
            //2 vectors are parallel if the angle between the vectors are 0 or 180 degrees
            float3 an = normalize(a), bn = normalize(b);
            float angle = acos(clamp((an.x * bn.x + an.y * bn.y + an.z * bn.z), -1f, 1f)) * 57.29578f;
            return (angle == 0f || angle == 180f);
        }

        #endregion

        #region Circle

        public static bool TryGetCircleIntersection(
            float2 centerA,
            float radiusA,
            float2 centerB,
            float radiusB,
            out float2 pt1,
            out float2 pt2)
        {

            float2 AB = float2(centerA.x - centerB.x, centerA.y - centerB.y);
            float d = sqrt(AB.x * AB.x + AB.y * AB.y);

            if (d <= (radiusA + radiusB) && d >= abs(radiusB - radiusA))
            {

                float ex = (centerB.x - centerA.x) / d, ey = (centerB.y - centerA.y) / d,
                    x = (radiusA * radiusA - radiusB * radiusB + d * d) / (2 * d),
                    y = sqrt(radiusA * radiusA - x * x),
                    xex = centerA.x + x * ex, xey = centerA.y + x * ey, yex = y * ex, yey = y * ey;

                pt1 = float2(xex - yey, xey + yex);
                pt2 = float2(xex + yey, xey - yex);

                return true;

            }
            else
            {
                // No Intersection, far outside or one circle within the other
                pt1 = pt2 = Vector2.zero;
                return false;
            }
        }

        public static bool CircleIntersects(
            float2 centerA,
            float radiusA,
            float2 centerB,
            float radiusB)
        {

            float2 AB = float2(centerA.x - centerB.x, centerA.y - centerB.y);
            float d = sqrt(AB.x * AB.x + AB.y * AB.y);

            if (d <= (radiusA + radiusB) && d >= abs(radiusB - radiusA))
                return true;
            else
                return false;
        }

        #endregion

        #region Triangle

        public static bool TriangleContainsXY(float3 pt, float3 a, float3 b, float3 c)
        {
            float A = 1 / 2 * (-b.y * c.x + a.y * (-b.x + c.x) + a.x * (b.y - c.y) + b.x * c.y);
            float sign = A < 0 ? -1 : 1;
            float s = (a.y * c.x - a.x * c.y + (c.y - a.y) * pt.x + (a.x - c.x) * pt.y) * sign;
            float t = (a.x * b.y - a.y * b.x + (a.y - b.y) * pt.x + (b.x - a.x) * pt.y) * sign;

            return s > 0 && t > 0 && (s + t) < 2 * A * sign;
        }

        public static bool TriangleContainsXZ(float3 pt, float3 a, float3 b, float3 c)
        {
            float A = 1 / 2 * (-b.z * c.x + a.z * (-b.x + c.x) + a.x * (b.z - c.z) + b.x * c.z);
            float sign = A < 0 ? -1 : 1;
            float s = (a.z * c.x - a.x * c.z + (c.z - a.z) * pt.x + (a.x - c.x) * pt.z) * sign;
            float t = (a.x * b.z - a.z * b.x + (a.z - b.z) * pt.x + (b.x - a.x) * pt.z) * sign;

            return s > 0 && t > 0 && (s + t) < 2 * A * sign;
        }

        public static float3 CircumSphere(float3 A, float3 B, float3 C)
        {
            float3 ac = C - A;
            float3 ab = B - A;
            float3 abXac = cross(ab, ac);

            // this is the vector from a TO the circumsphere center
            float3 toCircumsphereCenter = (cross(abXac, ab) * sqrt(ac) + cross(ac, abXac) * sqrt(ab)) / (2.0f * sqrt(abXac));
            //float circumsphereRadius = toCircumsphereCenter.magnitude;

            // The 3 space coords of the circumsphere center then:
            return A + toCircumsphereCenter; // now this is the actual 3space location
        }


        #endregion

        #region Matrices

        /// <summary>
        /// Extract translation from transform matrix.
        /// </summary>
        /// <param name="matrix">Transform matrix. This parameter is passed by reference
        /// to improve performance; no changes will be made to it.</param>
        /// <returns>
        /// Translation offset.
        /// </returns>
        public static Vector3 ExtractTranslationFromMatrix(ref Matrix4x4 matrix)
        {
            Vector3 translate;
            translate.x = matrix.m03;
            translate.y = matrix.m13;
            translate.z = matrix.m23;
            return translate;
        }

        /// <summary>
        /// Extract rotation quaternion from transform matrix.
        /// </summary>
        /// <param name="matrix">Transform matrix. This parameter is passed by reference
        /// to improve performance; no changes will be made to it.</param>
        /// <returns>
        /// Quaternion representation of rotation transform.
        /// </returns>
        public static Quaternion ExtractRotationFromMatrix(ref Matrix4x4 matrix)
        {
            Vector3 forward;
            forward.x = matrix.m02;
            forward.y = matrix.m12;
            forward.z = matrix.m22;

            Vector3 upwards;
            upwards.x = matrix.m01;
            upwards.y = matrix.m11;
            upwards.z = matrix.m21;

            return Quaternion.LookRotation(forward, upwards);
        }

        /// <summary>
        /// Extract scale from transform matrix.
        /// </summary>
        /// <param name="matrix">Transform matrix. This parameter is passed by reference
        /// to improve performance; no changes will be made to it.</param>
        /// <returns>
        /// Scale vector.
        /// </returns>
        public static Vector3 ExtractScaleFromMatrix(ref Matrix4x4 matrix)
        {
            Vector3 scale;
            scale.x = new Vector4(matrix.m00, matrix.m10, matrix.m20, matrix.m30).magnitude;
            scale.y = new Vector4(matrix.m01, matrix.m11, matrix.m21, matrix.m31).magnitude;
            scale.z = new Vector4(matrix.m02, matrix.m12, matrix.m22, matrix.m32).magnitude;
            return scale;
        }

        /// <summary>
        /// Extract position, rotation and scale from TRS matrix.
        /// </summary>
        /// <param name="matrix">Transform matrix. This parameter is passed by reference
        /// to improve performance; no changes will be made to it.</param>
        /// <param name="localPosition">Output position.</param>
        /// <param name="localRotation">Output rotation.</param>
        /// <param name="localScale">Output scale.</param>
        public static void DecomposeMatrix(ref Matrix4x4 matrix, out Vector3 localPosition, out Quaternion localRotation, out Vector3 localScale)
        {
            localPosition = ExtractTranslationFromMatrix(ref matrix);
            localRotation = ExtractRotationFromMatrix(ref matrix);
            localScale = ExtractScaleFromMatrix(ref matrix);
        }

        /// <summary>
        /// Set transform component from TRS matrix.
        /// </summary>
        /// <param name="transform">Transform component.</param>
        /// <param name="matrix">Transform matrix. This parameter is passed by reference
        /// to improve performance; no changes will be made to it.</param>
        public static void SetTransformFromMatrix(Transform transform, ref Matrix4x4 matrix)
        {
            transform.localPosition = ExtractTranslationFromMatrix(ref matrix);
            transform.localRotation = ExtractRotationFromMatrix(ref matrix);
            transform.localScale = ExtractScaleFromMatrix(ref matrix);
        }


        // EXTRAS!

        /// <summary>
        /// Identity quaternion.
        /// </summary>
        /// <remarks>
        /// <para>It is faster to access this variation than <c>Quaternion.identity</c>.</para>
        /// </remarks>
        public static readonly Quaternion IdentityQuaternion = Quaternion.identity;
        /// <summary>
        /// Identity matrix.
        /// </summary>
        /// <remarks>
        /// <para>It is faster to access this variation than <c>Matrix4x4.identity</c>.</para>
        /// </remarks>
        public static readonly Matrix4x4 IdentityMatrix = Matrix4x4.identity;

        /// <summary>
        /// Get translation matrix.
        /// </summary>
        /// <param name="offset">Translation offset.</param>
        /// <returns>
        /// The translation transform matrix.
        /// </returns>
        public static Matrix4x4 TranslationMatrix(Vector3 offset)
        {
            Matrix4x4 matrix = IdentityMatrix;
            matrix.m03 = offset.x;
            matrix.m13 = offset.y;
            matrix.m23 = offset.z;
            return matrix;
        }

        #endregion

    }
}
