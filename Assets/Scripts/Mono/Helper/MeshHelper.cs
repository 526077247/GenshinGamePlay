using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public static class MeshHelper
    {
        public class PointNode
        {
            public Vector2 Position;
            public PointNode PreviousNode;
            public PointNode NextNode;

            public PointNode(Vector2 position)
            {
                this.Position = position;
            }
        }
        public struct Triangle
        {
            public Vector2 a;
            public Vector2 b;
            public Vector2 c;
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
                return 0;//平角
            }
            else if (flag)
            {
                return 2;//劣角
            }
            else
            {
                return 1;//优角
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
            PointNode firstNode = new PointNode(points[0]);
            // 创建后续节点
            PointNode now = firstNode, previous;
            // Vector2[] points
            for (int i = 1; i < points.Length; i++)
            {
                previous = now;
                now = new PointNode(points[i]);
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
                if (IsInside(node.PreviousNode.Position,node.Position,node.NextNode.Position, now.Position))
                {
                    flag = true;
                    break;
                }
            }
            return flag;
        }

        private static PointNode RemovePoint(PointNode node)
        {
            var res = node.NextNode;
            node.PreviousNode.NextNode = res;
            return res;
        }

        private static Triangle GenTriangle(PointNode node)
        {
            return new Triangle()
            {
                a = node.PreviousNode.Position,
                b = node.Position,
                c = node.NextNode.Position
            };
        }
        
        /// <summary>
        /// 三角形化
        /// </summary>
        /// <returns></returns>
        public static Triangle[] Triangulate(Vector2[] points)
        {
            if (points.Length < 3)
            {
                return new Triangle[0];
            }
            else
            {
                // 节点数量
                int count = points.Length;
                // 确定方向
                bool isClockWise = IsClockWise(points);
                // 初始化节点
                PointNode curNode = GenPointNote(points);
                // 三角形数量
                int triangleCount = count - 2;
                // 获取三角形
                List<Triangle> triangles = new List<Triangle>();
                int angleType;
                while (triangles.Count < triangleCount)
                {
                    // 获取耳点
                    int i = 0, maxI = count - 1;
                    for (; i <= maxI; i++)
                    {
                        angleType = GetAngleType(curNode.Position,curNode.PreviousNode.Position,curNode.NextNode.Position,isClockWise);
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
                    // DebugDraw(curNode, count, triangles);
                    // 还需要分割耳点,但找不到ear
                    if (triangles.Count < triangleCount && i > maxI)
                    {
                        Debug.Log("找不到ear");
                        triangles.Clear();
                        break;
                    }
                }
                return triangles.ToArray();
            }
        }

    }
}