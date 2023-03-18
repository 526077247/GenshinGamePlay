using System.Collections.Generic;
using Nino.Serialization;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigTransposer
    {
        [NinoMember(1)]
        public Vector3 followOffset = new(0, 0, -10);
        [NinoMember(2)]
        [Range(0, 20)] public float xDamping = 1;
        [NinoMember(3)]
        [Range(0, 20)] public float yawDamping;
        [NinoMember(4)]
        [Range(0, 20)] public float yDamping = 1;
        [NinoMember(5)]
        [Range(0, 20)] public float zDamping = 1;
        
    }
}