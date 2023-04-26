using UnityEngine;

namespace TaoTie
{
    public struct SensibleInfo
    {
        public long SensibleID;
        public Vector3 Position;
        public Vector3 TargetablePosition;
        public Vector3 Direction;
        public float Distance;
        public bool HasLineOfSight;
        public bool IsCharacterEntity;
    }
}