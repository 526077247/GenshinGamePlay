using System.Collections.Generic;

using UnityEngine;

namespace TaoTie
{
    public class ConfigTransposer
    {
        [SerializeField] private Vector3 _followOffset = new(0, 0, -10);

        [Range(0, 20)] [SerializeField] private float _xDamping = 1;

        [Range(0, 20)] [SerializeField] private float _yawDamping;

        [Range(0, 20)] [SerializeField] private float _yDamping = 1;

        [Range(0, 20)] [SerializeField] private float _zDamping = 1;

        public Vector3 followOffset => _followOffset;
        public float xDamping => _xDamping;
        public float yDamping => _xDamping;
        public float zDamping => _xDamping;
        public float yawDamping => _yawDamping;
    }
}