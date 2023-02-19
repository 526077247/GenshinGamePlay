using UnityEngine;

namespace TaoTie
{
    public struct SensibleInfo
    {
        public long sensibleID;
        public Vector3 position;
        public Vector3 targetablePosition;
        public Vector3 direction;
        public float distance;
        public bool hasLineOfSight;
        public bool isCharacterEntity;
    }
}