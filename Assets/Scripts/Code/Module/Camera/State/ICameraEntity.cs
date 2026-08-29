using UnityEngine;

namespace TaoTie
{
    public interface ICameraEntity
    {
        public Vector3 Position { get; }
        public Quaternion Rotation { get; }
        public Vector3 Up { get; }
        public Vector3 Forward { get; }
    }
}