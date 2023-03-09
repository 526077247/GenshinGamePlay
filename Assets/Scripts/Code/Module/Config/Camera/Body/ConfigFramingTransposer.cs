using System.Collections.Generic;

using UnityEngine;

namespace TaoTie
{
    public class ConfigFramingTransposer
    {
        [SerializeField] [Min(0.01f)] private float _cameraDistance;

        [SerializeField] private Vector3 _trackedObjectOffset;

        public Vector3 trackedObjectOffset => _trackedObjectOffset;

        public float cameraDistance => _cameraDistance;
        
    }
}