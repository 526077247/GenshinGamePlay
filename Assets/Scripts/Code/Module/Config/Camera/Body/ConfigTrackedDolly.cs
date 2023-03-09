using System.Collections.Generic;

using UnityEngine;

namespace TaoTie
{
    public class ConfigTrackedDolly
    {
        [SerializeField] private float _xdamping = 0;
        [SerializeField] private float _ydamping = 0;
        [SerializeField] private float _zdamping = 0;

        public float xdamping => _xdamping;
        public float ydamping => _ydamping;
        public float zdamping => _zdamping;



    }
}