using System;
using System.Collections.Generic;
using Nino.Serialization;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize()]
    public partial class ConfigCameraRoute
    {
        [NinoMember(1)]
        public ConfigCameraRoutePoint[] Points;
        [NinoMember(2)]
        public int Resolution;
        [NinoMember(3)]
        public bool Loop;
    }
}