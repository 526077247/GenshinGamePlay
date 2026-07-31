using System.Collections.Generic;
using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif
using UnityEngine;

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigCameras
    {
        [ProtoMember(1)]
        public ConfigCamera DefaultCamera;
        [ProtoMember(2)]
        public ConfigCamera[] Cameras;
        [ProtoMember(3, IsRequired = true)] [HideReferenceObjectPicker]
        public ConfigBlender DefaultBlend = new ConfigBlender();
    }
}