using System.Collections.Generic;
using Nino.Serialization;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigFramingTransposer
    {
        [NinoMember(1)] [Min(0.01f)] public float CameraDistance;

        [NinoMember(2)] public Vector3 TrackedObjectOffset;
        
        
    }
}