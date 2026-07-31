using System.Collections.Generic;
using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif
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