using System.Collections.Generic;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigCameras
    {
        [NinoMember(1)]
        public ConfigCamera DefaultCamera;
        [NinoMember(2)]
        public ConfigCamera[] Cameras;

    }
}