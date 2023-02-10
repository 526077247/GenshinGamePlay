using System;
using LitJson.Extensions;
using UnityEngine;

namespace TaoTie
{
    public abstract class ConfigFsmClip
    {
        public float StartTime = 0.0f;
        public float Length = 0.0f;
        

        public abstract FsmClip CreateClip(FsmState state);
    }
}