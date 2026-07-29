using System;
using TaoTie.LitJson.Extensions;
using ProtoBuf;
using UnityEngine;

namespace TaoTie
{
    [ProtoContract]
    [ProtoInclude(100, typeof(ConfigAttachAbilityClip))]
    [ProtoInclude(101, typeof(ConfigExecuteAbilityClip))]
    [ProtoInclude(102, typeof(ConfigTriggerClip))]
    public abstract partial class ConfigFsmClip
    {
        [ProtoMember(1, IsRequired = true)]
        public float StartTime = 0.0f;
        [ProtoMember(2, IsRequired = true)]
        public float Length = 0.0f;
    }
}