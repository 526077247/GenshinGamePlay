using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)] [LabelText("立方")]
    public partial class ConfigObbShape: ConfigShape
    {
        [NinoMember(1)]
        public Vector3 Size;

        public override Collider CreateCollider(GameObject obj, bool isTrigger)
        {
            var collider = obj.AddComponent<BoxCollider>();
            collider.isTrigger = isTrigger;
            collider.size = Size;
            return collider;
        }

        public override bool Contains(Vector3 target)
        {
            var x = Size.x / 2;
            var y = Size.y / 2;
            var z = Size.z / 2;
            return -x <= target.x && target.x <= x
                                  && -y <= target.y && target.y <= y
                                  && -z < target.z && target.z < z;
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
        public override float Distance(Vector3 target)
        {
            var distance = Mathf.Sqrt(SqrMagnitude(target, out bool inner));
            return inner ? -distance : distance;
        }
        public override float SqrMagnitude(Vector3 target)
        {
            return SqrMagnitude(target, out _);
        }
        
        public override float SqrMagnitude(Vector3 target, out bool inner)
        {
            float sqrMagnitude = 0;
            var x = Size.x / 2;
            var y = Size.y / 2;
            var z = Size.z / 2;
            float minInner = float.MaxValue;
            if (target.x <= -x)
            {
                sqrMagnitude += Mathf.Pow(-x - target.x, 2);
            }
            else if (x <= target.x)
            {
                sqrMagnitude += Mathf.Pow(target.x - x, 2);
            }
            else
            {
                var l = Mathf.Pow(-x - target.x, 2);
                var r = Mathf.Pow(target.x - x, 2);
                minInner = Mathf.Min(l < r ? l : r, minInner);
            }
            if (target.y <= -y)
            {
                sqrMagnitude += Mathf.Pow(-y - target.y, 2);
            }
            else if (y <= target.y)
            {
                sqrMagnitude += Mathf.Pow(target.y - y, 2);
            }
            else
            {
                var l = Mathf.Pow(-y - target.y, 2);
                var r = Mathf.Pow(target.y - y, 2);
                minInner = Mathf.Min(l < r ? l : r, minInner);
            }
            if (target.z <= -z)
            {
                sqrMagnitude += Mathf.Pow(-z - target.z, 2);
            }
            else if (z <= target.z)
            {
                sqrMagnitude += Mathf.Pow(target.z - z, 2);
            }
            else
            {
                var l = Mathf.Pow(-z - target.z, 2);
                var r = Mathf.Pow(target.z - z, 2);
                minInner = Mathf.Min(l < r ? l : r, minInner);
            }
            inner = sqrMagnitude <= 0;
            return sqrMagnitude > 0 ? sqrMagnitude : minInner;
        }

        public override int RaycastHitInfo(Vector3 pos, Quaternion rot, EntityType[] filter, out HitInfo[] hitInfos)
        {
            return PhysicsHelper.OverlapBoxNonAllocHitInfo(pos, Size * 0.5f, rot, filter, CheckHitLayerType.OnlyHitBox,
                out hitInfos);
        }
        
        
        public override float GetAABBRange()
        {
            return Size.magnitude;
        }
    }
}