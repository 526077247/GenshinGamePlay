using System;
using LitJson.Extensions;
using Nino.Core;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
    public abstract partial class ConfigFsmClip
    {
        [NinoMember(1)]
        public float StartTime = 0.0f;
        [NinoMember(2)]
        public float Length = 0.0f;
    }
}