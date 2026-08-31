using System;
using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif

namespace TaoTie
{
    [LabelText("当移除组之后")]
    [ProtoContract]
    public partial class ConfigSuiteUnloadEventTrigger : ConfigSceneGroupTrigger<SuiteUnloadEvent>
    {

    }
}
