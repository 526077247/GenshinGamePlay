using System.Collections.Generic;

using UnityEngine;

namespace TaoTie
{
    public class ConfigHardLockToTarget
    {
        [SerializeField] private float _damping = 0;

        public float damping => _damping;
    }
}