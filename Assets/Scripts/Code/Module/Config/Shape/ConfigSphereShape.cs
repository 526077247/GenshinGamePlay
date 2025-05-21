using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)] [LabelText("球")]
    public partial class ConfigSphereShape: ConfigShape
    {
        [NinoMember(1)]
        public float Radius;

        public override Collider CreateCollider(GameObject obj, bool isTrigger)
        {
            var collider = obj.AddComponent<SphereCollider>();
            collider.isTrigger = isTrigger;
            collider.radius = Radius;
            return collider;
        }

        public override bool Contains(Vector3 target)
        {
            return target.sqrMagnitude <= Radius * Radius;
        }
        /// <summary>
        /// 线与形状相交
        /// </summary>
        /// <param name="start">转换过坐标系的点</param>
        /// <param name="end">转换过坐标系的点</param>
        /// <returns></returns>
        public override bool ContainsLine(Vector3 start, Vector3 end)
        {
            float dis = MeshHelper.DistanceParallel(start,end, Vector3.zero);
            return dis <= Radius;
        }
        public override float Distance(Vector3 target)
        {
            return target.magnitude - Radius;
        }

        public override float SqrMagnitude(Vector3 target)
        {
            return (target - target.normalized*Radius).sqrMagnitude;
        }

        public override float SqrMagnitude(Vector3 target, out bool inner)
        {
            inner = Contains(target);
            return (target - target.normalized * Radius).sqrMagnitude;
        }

        public override int RaycastHitInfo(Vector3 pos, Quaternion rot, EntityType[] filter, out HitInfo[] hitInfos)
        {
            return PhysicsHelper.OverlapSphereNonAllocHitInfo(pos, Radius, filter, CheckHitLayerType.OnlyHitBox,
                out hitInfos);
        }

        public override float GetAABBRange()
        {
            return Radius * 2;
        }
    }
}