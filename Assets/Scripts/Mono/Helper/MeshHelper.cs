using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public static class MeshHelper
    {
        #region 多边形
        private static SortedSet<PointNode> sortedSet = new SortedSet<PointNode>(new PointNodeComparer());

        class PointNodeComparer : IComparer<PointNode>
        {
            public int Compare(PointNode a, PointNode b)
            {
                return a.Index - b.Index;
            }
        }

        private class PointNode : IDisposable
        {
            public Vector2 Position;
            public PointNode PreviousNode;
            public PointNode NextNode;
            public int Index;

            public static PointNode Create(Vector2 position, int index)
            {
                PointNode res = ObjectPool.Instance.Fetch<PointNode>();
                res.Position = position;
                res.Index = index;
                return res;
            }

            public void Dispose()
            {
                PreviousNode = null;
                NextNode = null;
            }
        }

        private struct Triangle
        {
            public PointNode a;
            public PointNode b;
            public PointNode c;
        }

        private static bool IsInside(Vector2 c, Vector2 a, Vector2 b, Vector2 p)
        {
            // p点是否在abc三角形内
            var c1 = (b.x - a.x) * (p.y - b.y) - (b.y - a.y) * (p.x - b.x);
            var c2 = (c.x - b.x) * (p.y - c.y) - (c.y - b.y) * (p.x - c.x);
            var c3 = (a.x - c.x) * (p.y - a.y) - (a.y - c.y) * (p.x - a.x);
            return
                (c1 > 0f && c2 >= 0f && c3 >= 0f) ||
                (c1 < 0f && c2 <= 0f && c3 <= 0f);
        }

        /// <summary>
        /// 判断角的类型,oa & ob 之间的夹角，（右手法则）
        /// </summary>
        private static int GetAngleType(Vector2 o, Vector2 a, Vector2 b, bool isClockWise)
        {
            float f = (a.x - o.x) * (b.y - o.y) - (a.y - o.y) * (b.x - o.x);
            bool flag = isClockWise ? f > 0 : f < 0;
            if (f == 0)
            {
                return 0; //平角
            }
            else if (flag)
            {
                return 2; //劣角
            }
            else
            {
                return 1; //优角
            }
        }

        private static bool IsClockWise(Vector2[] points)
        {
            // 通过计算叉乘来确定方向
            float sum = 0f;
            double count = points.Length;
            Vector3 va, vb;
            for (int i = 0; i < points.Length; i++)
            {
                va = points[i];
                vb = (i == count - 1) ? points[0] : points[i + 1];
                sum += va.x * vb.y - va.y * vb.x;
            }

            return sum < 0;
        }

        /// <summary>
        /// 生成点节点
        /// </summary>
        private static PointNode GenPointNote(Vector2[] points)
        {
            // 创建第一个节点
            PointNode firstNode = PointNode.Create(points[0], 0);
            // 创建后续节点
            PointNode now = firstNode, previous;
            // Vector2[] points
            for (int i = 1; i < points.Length; i++)
            {
                previous = now;
                now = PointNode.Create(points[i], i);
                // 关联
                now.PreviousNode = previous;
                previous.NextNode = now;
            }

            // 关联头尾
            firstNode.PreviousNode = now;
            now.NextNode = firstNode;
            return firstNode;
        }

        /// <summary>
        /// 当前点组成的三角形，是否包含其他点
        /// </summary>
        private static bool IsInsideOtherPoint(PointNode node, int count)
        {
            bool flag = false;
            int checkCount = count - 3;
            //now 第一个开始校验其实是node.NextNode.NextNode
            PointNode now = node.NextNode;
            for (int i = 0; i < checkCount; i++)
            {
                now = now.NextNode;
                if (IsInside(node.PreviousNode.Position, node.Position, node.NextNode.Position, now.Position))
                {
                    flag = true;
                    break;
                }
            }

            return flag;
        }

        private static PointNode RemovePoint(PointNode node)
        {
            var next = node.NextNode;
            var pre = node.PreviousNode;
            node.PreviousNode.NextNode = next;
            node.NextNode.PreviousNode = pre;
            node.Dispose();
            return next;
        }

        private static Triangle GenTriangle(PointNode node)
        {
            return new Triangle()
            {
                a = node.PreviousNode,
                b = node,
                c = node.NextNode
            };
        }

        /// <summary>
        /// 三角形化
        /// </summary>
        /// <returns></returns>
        private static Triangle[] Triangulate(Vector2[] points)
        {
            if (points.Length < 3)
            {
                return Array.Empty<Triangle>();
            }

            // 节点数量
            int count = points.Length;
            // 确定方向
            bool isClockWise = IsClockWise(points);
            // 初始化节点
            PointNode curNode = GenPointNote(points);
            // 三角形数量
            int triangleCount = count - 2;
            // 获取三角形
            using (ListComponent<Triangle> triangles = ListComponent<Triangle>.Create())
            {
                int angleType;
                while (triangles.Count < triangleCount)
                {
                    // 获取耳点
                    int i = 0, maxI = count - 1;
                    for (; i <= maxI; i++)
                    {
                        angleType = GetAngleType(curNode.Position, curNode.PreviousNode.Position,
                            curNode.NextNode.Position, isClockWise);
                        if (angleType == 0)
                        {
                            // 等于180，不可能为耳点
                            // 移除当前点，三角形数量少一个
                            curNode = RemovePoint(curNode);
                            count--;
                            triangleCount--;
                        }
                        else if (angleType == 1)
                        {
                            // 大于180，不可能为耳点
                            curNode = curNode.NextNode;
                        }
                        else if (IsInsideOtherPoint(curNode, count))
                        {
                            //包含其他点，不可能为耳点
                            curNode = curNode.NextNode;
                        }
                        else
                        {
                            // 当前点就是ear，添加三角形,移除当前节点
                            triangles.Add(GenTriangle(curNode));
                            curNode = RemovePoint(curNode);
                            count--;
                            break;
                        }
                    }

                    // 还需要分割耳点,但找不到ear
                    if (triangles.Count < triangleCount && i > maxI)
                    {
                        Log.Info("找不到ear");
                        triangles.Clear();
                        break;
                    }
                }

                return triangles.ToArray();
            }
        }

        /// <summary>
        /// 获取多边形Mesh数据
        /// </summary>
        /// <param name="points"></param>
        /// <param name="triangles"></param>
        /// <param name="vertices"></param>
        public static void GetPolygonMeshData(Vector2[] points, List<int> triangles, List<Vector3> vertices)
        {
            int i;
            var triangulates = Triangulate(points);
            for (i = 0; i < triangulates.Length; i++)
            {
                var triangulate = triangulates[i];
                sortedSet.Add(triangulate.a);
                sortedSet.Add(triangulate.b);
                sortedSet.Add(triangulate.c);
            }
            var start = vertices.Count;
            i = 0;
            foreach (var item in sortedSet)
            {
                item.Index = start + i;
                vertices.Add(new Vector3(item.Position.x, 0, item.Position.y));
                i++;
            }

            for (i = 0; i < triangulates.Length; i++)
            {
                var triangulate = triangulates[i];
                triangles.Add(triangulate.a.Index);
                triangles.Add(triangulate.b.Index);
                triangles.Add(triangulate.c.Index);
            }

            foreach (var item in sortedSet)
            {
                item.Dispose();
            }
            sortedSet.Clear();
        }
        
        #endregion

        /// <summary>
        /// 已知线段AB点坐标 求P到AB最短距离
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <param name="P"></param>
        /// <returns></returns>
        public static float DistanceParallel(Vector3 A, Vector3 B, Vector3 P)
        {
            float res;
            Vector3 AB = new Vector3(A[0] - B[0], A[1] - B[1], A[2] - B[2]);
            Vector3 AP = new Vector3(A[0] - P[0], A[1] - P[1], A[2] - P[2]);
            float r = Vector3.Dot(AP, AB) / AB.sqrMagnitude;
            if (r <= 0)
            {
                res = AP.magnitude; //在BA延长线上
            }
            else if (r >= 1)
            {
                res = new Vector3(B[0] - P[0], B[1] - P[1], B[2] - P[2]).magnitude; //在AB延长线上
            }
            else
            {
                //在AB上
                res = Vector3.Cross(AP, AB).magnitude / AB.magnitude;
            }

            return res;
        }

        static bool OnSegment(Vector2 p, Vector2 q, Vector2 r)
        {
            if (q.x <= Mathf.Max(p.x, r.x) && q.x >= Mathf.Min(p.x, r.x) &&
                q.y <= Mathf.Max(p.y, r.y) && q.y >= Mathf.Min(p.y, r.y))
                return true;

            return false;
        }

        static int Orientation(Vector2 p, Vector2 q, Vector2 r)
        {
            var val = (q.y - p.y) * (r.x - q.x) -
                      (q.x - p.x) * (r.y - q.y);

            if (val == 0) return 0;
            return (val > 0) ? 1 : 2;
        }
        
        /// <summary>
        /// 求p1,q1 和 p2,q2是否相交
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="q1"></param>
        /// <param name="p2"></param>
        /// <param name="q2"></param>
        /// <returns></returns>
        public static bool IsIntersect(Vector2 p1, Vector2 q1, Vector2 p2, Vector2 q2)
        {
            int o1 = Orientation(p1, q1, p2);
            int o2 = Orientation(p1, q1, q2);
            int o3 = Orientation(p2, q2, p1);
            int o4 = Orientation(p2, q2, q1);

            if (o1 != o2 && o3 != o4)
                return true;

            if (o1 == 0 && OnSegment(p1, p2, q1)) return true;

            if (o2 == 0 && OnSegment(p1, q2, q1)) return true;

            if (o3 == 0 && OnSegment(p2, p1, q2)) return true;

            if (o4 == 0 && OnSegment(p2, q1, q2)) return true;

            return false;
        }
    }
}