using System;
using ProtoBuf;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [LabelText("当添加或附加组之后")]
    [ProtoContract]
    public partial class ConfigSuiteLoadEventTrigger : ConfigSceneGroupTrigger<SuiteLoadEvent>
    {

    }
}