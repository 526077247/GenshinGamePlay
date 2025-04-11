using System.Collections.Generic;
using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [LabelText("多棱柱")]
    [NinoType(false)]
    public partial class ConfigSceneGroupZonePolygon: ConfigSceneGroupZone
    {
        [NinoMember(5)]
        public Vector3 Rotation;
        [NinoMember(6)][LabelText("俯视图即底面二维坐标")][NotNull]
        public Vector2[] Points;
        [NinoMember(7)][MinValue(0.1f)]
        public float Height = 1;
        
        public override Zone CreateZone(SceneGroup sceneGroup)
        {
            Vector3 position;
            Quaternion rotation;
            if (IsLocal)
            {
                position = Quaternion.Euler(sceneGroup.Rotation) * Position + sceneGroup.Position;
                rotation = Quaternion.Euler(sceneGroup.Rotation + Rotation);
            }
            else
            {
                position = Position;
                rotation = Quaternion.Euler(Rotation);
            }
            var entity = sceneGroup.Parent.CreateEntity<Zone>();
            var obj = entity.GameObject;
            obj.name = "ZonePolygon";
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            var collider = obj.AddComponent<MeshCollider>();
            collider.convex = true;
            collider.isTrigger = true;
            
            collider.sharedMesh = CreateMesh();
            entity.DynMesh = collider.sharedMesh;
            entity.Collider = collider;
            entity.AddComponent<SceneGroupZoneComponent, int, long, GameObject>(LocalId, sceneGroup.Id, obj);
            return entity;
        }

        private Mesh CreateMesh()
        {
            Mesh mesh = new Mesh();
            mesh.name = "ZonePolygon";
            mesh.triangles = null;
            mesh.uv = null;
            mesh.vertices = null;
            mesh.tangents = null;

            Vector3[] vertices = new Vector3[Points.Length * 2 + 2];
            vertices[0] = -Vector3.up * Height/2;
            vertices[1] = Vector3.up * Height/2;

            for (int i = 0; i < Points.Length; i++)
            {
                var pos = new Vector3(Points[i].x, 0, Points[i].y);
                vertices[i + 2] = vertices[0] + pos;
                vertices[i + Points.Length + 2] = vertices[1] + pos;
            }

            int[] triangles = new int[Points.Length * 4 * 3];
            for (int i = 0; i < Points.Length; i++)
            {
                int bottomFace = i * 3;
                triangles[bottomFace] = 0;
                triangles[bottomFace + 1] = i + 2;

                int topFace = Points.Length * 3 + i * 3;
                triangles[topFace] = 1;
                triangles[topFace + 2] = i + Points.Length + 2;

                int sideFace = Points.Length * 3 * 2 + 6 * i;
                triangles[sideFace] = i + 2;
                triangles[sideFace + 1] = i + 2 + Points.Length;
                triangles[sideFace + 4] = i + 2 + Points.Length;

                if (i >= Points.Length - 1)
                {
                    triangles[bottomFace + 2] = 2;
                    triangles[topFace + 1] = 2 + Points.Length;
                    triangles[sideFace + 2] = 2;
                    triangles[sideFace + 3] = 2;
                    triangles[sideFace + 5] = 2 + Points.Length;
                }
                else
                {
                    triangles[bottomFace + 2] = i + 2 + 1;
                    triangles[topFace + 1] = i + Points.Length + 2 + 1;
                    triangles[sideFace + 2] = i + 2 + 1;
                    triangles[sideFace + 3] = i + 2 + 1;
                    triangles[sideFace + 5] = i + 2 + Points.Length + 1;
                }
            }
            
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            return mesh;
        }
    }
}