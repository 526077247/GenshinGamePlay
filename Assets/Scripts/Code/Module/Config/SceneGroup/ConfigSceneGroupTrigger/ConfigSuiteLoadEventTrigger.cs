﻿using System;
using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [LabelText("当添加或附加组之后")]
    [NinoSerialize]
    public partial class ConfigSuiteLoadEventTrigger : ConfigSceneGroupTrigger<SuiteLoadEvent>
    {

    }
}