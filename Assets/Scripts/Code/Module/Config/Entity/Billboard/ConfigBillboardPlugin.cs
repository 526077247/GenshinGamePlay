using LitJson.Extensions;
using Nino.Core;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
    public abstract partial class ConfigBillboardPlugin
    {
        [NinoMember(1)]
        public Vector3 Offset;
    }
}