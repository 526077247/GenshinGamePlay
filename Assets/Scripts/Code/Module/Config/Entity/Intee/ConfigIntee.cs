using System.Collections.Generic;
using ProtoBuf;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 交互面板配置
    /// </summary>
    [ProtoContract]
    public partial class ConfigIntee
    {
        [ProtoMember(1)]
        public float Radius;
        [ProtoMember(2)]
        public float Height;
        [ProtoMember(3)]
        public Vector3 Offset;
        [ProtoMember(4)] 
        public ConfigInteeItem[] Params;
        [ProtoMember(5)] [LabelText("默认启用")]
        public bool DefaultEnable;
    }
}