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

        public override void GetMeshData(List<int> triangles, List<Vector3> vertices)
        {
            var triangulates = MeshHelper.Triangulate(Points);
            using (DictionaryComponent<Vector2, int> temp = DictionaryComponent<Vector2, int>.Create())
            {
                for (int i = 0; i < triangulates.Length; i++)
                {
                    var triangulate = triangulates[i];
                    if (!temp.TryGetValue(triangulate.a, out var index1))
                    {
                        index1 = vertices.Count;
                        temp.Add(triangulate.a, index1);
                        vertices.Add(new Vector3(triangulate.a.x, 0, triangulate.a.y));
                    }

                    if (!temp.TryGetValue(triangulate.b, out var index2))
                    {
                        index2 = vertices.Count;
                        temp.Add(triangulate.b, index2);
                        vertices.Add(new Vector3(triangulate.b.x, 0, triangulate.b.y));
                    }

                    if (!temp.TryGetValue(triangulate.c, out var index3))
                    {
                        index3 = vertices.Count;
                        temp.Add(triangulate.c, index3);
                        vertices.Add(new Vector3(triangulate.c.x, 0, triangulate.c.y));
                    }

                    triangles.Add(index1);
                    triangles.Add(index2);
                    triangles.Add(index3);
                }
            }

        }
    }
}