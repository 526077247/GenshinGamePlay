using System.Collections.Generic;
using Nino.Core;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigPolygonShape2D : ConfigShape2D
    {
        [NinoMember(1)] [NotNull] public Vector2[] Points;

        public override bool Contains(Vector2 target)
        {
            int num = Points.Length;
            bool flag = true;
            for (int i = 0; i < num; i++)
            {
                if ((target == Points[i]) || (target == Points[(i + 1) % num]))
                    return true;
                if ((target.y >= Points[i].y && target.y < Points[(i + 1) % num].y) ||
                    (target.y >= Points[(i + 1) % num].y && target.y < Points[i].y))
                {
                    float x = (Points[i].x - Points[(i + 1) % num].x) * (target.y - Points[i].y)
                        / (Points[i].y - Points[(i + 1) % num].y) + Points[i].x;
                    if (Mathf.Abs(x - target.x) < 1e-4)
                        return true;
                    if (x > target.x)
                        flag = !flag;
                }
            }

            return flag;
        }

        public override bool ContainsLine(Vector2 start, Vector2 end)
        {
            int num = Points.Length;
            for (int i = 0; i < num; i++)
            {
                if ((start == Points[i]) || (start == Points[(i + 1) % num]))
                    return true;
                if (MeshHelper.IsIntersect(start, end, Points[i], Points[(i + 1) % num]))
                    return true;
            }
            return false;
        }


        public override float Distance(Vector2 target)
        {
            var distance = Mathf.Sqrt(SqrMagnitude(target, out bool inner));
            return inner ? -distance : distance;
        }

        public override float SqrMagnitude(Vector2 target)
        {
            // Vector2 closePoint = Points[0];
            float minSqrMagnitude = float.MaxValue;
            for (int i = 0; i < Points.Length; i++)
            {
                var p1 = Points[i];
                var p2 = Points[(i + 1) % Points.Length];
                var pp = GetProjectPoint(target, p1, p2);
                if ((pp.x - p1.x) * (pp.x - p2.x) <= 0) // 投影在线段范围内
                {
                    var ppsm = Vector2.SqrMagnitude(target - pp);
                    if (i == 0)
                    {
                        // closePoint = pp;
                        minSqrMagnitude = ppsm;
                    }
                    else
                    {
                        if (ppsm < minSqrMagnitude)
                        {
                            // closePoint = pp;
                            minSqrMagnitude = ppsm;
                        }
                    }
                }
                else if (i != Points.Length-1)
                {
                    var p2sm = Vector2.SqrMagnitude(target - p2);
                    if (p2sm < minSqrMagnitude)
                    {
                        // closePoint = p2;
                        minSqrMagnitude = p2sm;
                    }
                }
            }
            return minSqrMagnitude;
        }

        public override float SqrMagnitude(Vector2 target, out bool inner)
        {
            inner = Contains(target);
            return SqrMagnitude(target);
        }

        public override void GetMeshData(List<int> triangles, List<Vector3> vertices)
        {
            MeshHelper.GetPolygonMeshData(Points, triangles, vertices);
        }
        
        Vector2 GetProjectPoint(Vector2 point, Vector2 start, Vector2 end) {
            Vector2 sp = point - start;
            Vector2 se = end - start;
            Vector2 project = Vector3.Project (sp, se);
            Vector2 line = sp - project;
            Vector2 projectPoint = point + line;
            return projectPoint;
        }

        public override float GetAABBRange()
        {
            if (Points == null || Points.Length == 0) return 0;
            float minSqrDis = float.MaxValue;
            for (int i = 0; i < Points.Length - 1; i++)
            {
                for (int j = i + 1; j < Points.Length; j++)
                {
                    var dis = Vector2.SqrMagnitude(Points[i] - Points[j]);
                    if (dis < minSqrDis)
                    {
                        minSqrDis = dis;
                    }
                }
            }
            return Mathf.Sqrt(minSqrDis);
        }
    }
}