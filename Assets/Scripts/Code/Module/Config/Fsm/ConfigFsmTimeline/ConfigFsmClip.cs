using System;
using LitJson.Extensions;
using Nino.Serialization;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public abstract partial class ConfigFsmClip
    {
        [NinoMember(1)]
        public float StartTime = 0.0f;
        [NinoMember(2)]
        public float Length = 0.0f;
        

        public abstract FsmClip CreateClip(FsmState state);
    }
}