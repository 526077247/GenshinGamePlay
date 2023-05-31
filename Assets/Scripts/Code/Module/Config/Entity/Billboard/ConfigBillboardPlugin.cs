using LitJson.Extensions;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace TaoTie
{
    public abstract partial class ConfigBillboardPlugin
    {
        [NinoMember(1)]
        public Vector3 Offset;
    }
}