using System.Collections.Generic;
using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
    public class ConfigInput
    {
        [NinoMember(1)][LabelText("按键默认绑定")]
        [TableList]
        public ConfigInputBinding[] Config;
    }
}