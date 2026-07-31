using System;
using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif

namespace TaoTie
{
    [LabelText("当有实体离开区域")]
    [ProtoContract]
    public partial class ConfigExitZoneEventTrigger : ConfigSceneGroupTrigger<ExitZoneEvent>
    {

    }
}