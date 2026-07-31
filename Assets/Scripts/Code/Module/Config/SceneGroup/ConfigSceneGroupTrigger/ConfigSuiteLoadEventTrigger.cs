using System;
using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif

namespace TaoTie
{
    [LabelText("当添加或附加组之后")]
    [ProtoContract]
    public partial class ConfigSuiteLoadEventTrigger : ConfigSceneGroupTrigger<SuiteLoadEvent>
    {

    }
}