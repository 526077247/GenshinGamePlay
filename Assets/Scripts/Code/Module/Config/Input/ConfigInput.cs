using System.Collections.Generic;
using ProtoBuf;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [ProtoContract]
    public class ConfigInput
    {
        [ProtoMember(1)][LabelText("按键默认绑定")]
        [TableList]
        public ConfigInputBinding[] Config;
    }
}