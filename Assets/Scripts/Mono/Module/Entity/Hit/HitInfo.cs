using UnityEngine;

namespace TaoTie
{
    public struct HitInfo
    {
        public long EntityId;
        public Vector3 HitPos;
        public Vector3 HitDir;
        public float Distance;
        public HitBoxType HitBoxType;
    }
}