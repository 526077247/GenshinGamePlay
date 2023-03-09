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
        public ConfigFreeLookCamera defaultCamera;
        [NinoMember(2)]
        public ConfigCamera[] cameras;
        [NinoMember(3)]
        [NotNull] public BlendDefinition defaultBlend = new BlendDefinition();
        [NinoMember(4)]
        [TableList] public CustomBlend[] customSetting;
        
    }
}