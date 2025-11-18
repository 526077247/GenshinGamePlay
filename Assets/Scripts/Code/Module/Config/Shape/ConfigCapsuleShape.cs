using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [LabelText("垂直胶囊体")]
    [NinoType(false)]
    public partial class ConfigCapsuleShape: ConfigShape
    {
        [NinoMember(1)]
        public float Height;
        [NinoMember(2)]
        public float Radius;
        
        public override bool Contains(Vector3 target)
        {
            var halfH = Height / 2;
            if (-halfH <= target.y && target.y <= halfH)
            {
                return new Vector2(target.x,target.z).sqrMagnitude < Radius*Radius;
            }

            if (target.y < -halfH)
            {
                return Vector3.SqrMagnitude(Vector3.up * -halfH - target) < Radius * Radius;
            }
            return Vector3.SqrMagnitude(Vector3.up * halfH - target) < Radius * Radius;
        }

        /// <summary>
        /// 线与形状相交
        /// </summary>
        /// <param name="start">转换过坐标系的点</param>
        /// <param name="end">转换过坐标系的点</param>
        /// <returns></returns>
        public override bool ContainsLine(Vector3 start, Vector3 end)
        {
            //todo:
            return false;
        }
        public override Collider CreateCollider(GameObject obj, bool isTrigger)
        {
            var collider = obj.AddComponent<CapsuleCollider>();
            collider.isTrigger = isTrigger;
            collider.height = Height;
            collider.radius = Radius;
            return collider;
        }

        public override float Distance(Vector3 target)
        {
            var halfH = Height / 2;
            if (-halfH <= target.y && target.y <= halfH)
            {
                return new Vector2(target.x,target.z).magnitude - Radius;
            }

            if (target.y < -halfH)
            {
                return Vector3.Magnitude(Vector3.up * -halfH - target) - Radius;
            }
            return Vector3.Magnitude(Vector3.up * halfH - target) - Radius;
        }

        public override float SqrMagnitude(Vector3 target)
        {
            var halfH = Height / 2;
            if (-halfH <= target.y && target.y <= halfH)
            {
                var target2d = new Vector2(target.x, target.z);
                return (target2d - target2d.normalized * Radius).sqrMagnitude;
            }

            if (target.y < -halfH)
            {
                target -= Vector3.up * -halfH;
                return (target - target.normalized * Radius).sqrMagnitude;
            }

            target -= Vector3.up * halfH;
            return (target - target.normalized * Radius).sqrMagnitude;
        }
        
        public override float SqrMagnitude(Vector3 target, out bool inner)
        {
            inner = false;
            var halfH = Height / 2;
            if (-halfH <= target.y && target.y <= halfH)
            {
                var target2d = new Vector2(target.x, target.z);
                inner = target2d.sqrMagnitude < Radius * Radius;
                return (target2d - target2d.normalized * Radius).sqrMagnitude;
            }

            if (target.y < -halfH)
            {
                target -= Vector3.up * -halfH;
                return (target - target.normalized * Radius).sqrMagnitude;
            }

            target -= Vector3.up * halfH;
            return (target - target.normalized * Radius).sqrMagnitude;
        }

        public override int RaycastHitInfo(Vector3 pos, Quaternion rot, EntityType[] filter, out HitInfo[] hitInfos)
        {
            var halfH = Vector3.up * Height * 0.5f;
            var p1 = rot * (pos + halfH);
            var p2 = rot * (pos - halfH);
            return PhysicsHelper.OverlapCapsuleNonAllocHitInfo(p1, p2, Radius, filter, CheckHitLayerType.OnlyHitBox,
                out hitInfos);
        }

        public override float GetAABBRange()
        {
            return Mathf.Sqrt(Height * Height + Radius * Radius * 4);
        }
    }
}