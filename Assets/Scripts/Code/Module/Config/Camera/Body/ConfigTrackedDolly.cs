using System.Collections.Generic;
using Nino.Serialization;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigTrackedDolly
    {
        [NinoMember(1)]
        public float xdamping;
        [NinoMember(2)]
        public float ydamping;
        [NinoMember(3)]
        public float zdamping;
    }
}