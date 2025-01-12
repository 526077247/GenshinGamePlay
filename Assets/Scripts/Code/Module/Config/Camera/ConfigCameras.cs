using System.Collections.Generic;
using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigCameras
    {
        [NinoMember(1)]
        public ConfigCamera DefaultCamera;
        [NinoMember(2)]
        public ConfigCamera[] Cameras;
        [NinoMember(3)] [HideReferenceObjectPicker]
        public ConfigBlender DefaultBlend = new ConfigBlender();
    }
}