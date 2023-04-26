using System.Collections.Generic;
using Nino.Serialization;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigTrackedDolly
    {
        [NinoMember(1)]
        public float XDamping;
        [NinoMember(2)]
        public float YDamping;
        [NinoMember(3)]
        public float ZDamping;
    }
}