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
        public ConfigCameraRoutePoint[] points;
        [NinoMember(2)]
        public int resolution;
        [NinoMember(3)]
        public bool loop;
    }
}