using System.Collections.Generic;
using Nino.Serialization;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize()]
    public class ConfigCameraRoutePoint
    {
        [NinoMember(1)]
        public Vector3 position;
        [NinoMember(2)]
        public float roll;
    }
}