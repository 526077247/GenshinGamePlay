using System.Collections.Generic;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 交互面板配置
    /// </summary>
    [NinoSerialize]
    public partial class ConfigIntee
    {
        [NinoMember(1)]
        public float Radius;
        [NinoMember(2)]
        public float Height;
        [NinoMember(3)]
        public Vector3 Offset;
        [NinoMember(4)] 
        public ConfigInteeItem[] Params;
        [NinoMember(5)] [LabelText("默认启用")]
        public bool DefaultEnable;
    }
}