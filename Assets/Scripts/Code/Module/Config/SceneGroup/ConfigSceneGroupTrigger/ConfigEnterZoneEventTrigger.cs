using System;
using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif

namespace TaoTie
{
    [LabelText("当有实体进入区域")]
    [ProtoContract]
    public partial class ConfigEnterZoneEventTrigger : ConfigSceneGroupTrigger<EnterZoneEvent>
    {

    }
}