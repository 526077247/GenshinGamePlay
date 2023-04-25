using System;
using LitJson.Extensions;
using Nino.Serialization;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public abstract partial class ConfigFsmClip
    {
        
        public float StartTime = 0.0f;
        
        public float Length = 0.0f;
        

        public abstract FsmClip CreateClip(FsmState state);
    }
}