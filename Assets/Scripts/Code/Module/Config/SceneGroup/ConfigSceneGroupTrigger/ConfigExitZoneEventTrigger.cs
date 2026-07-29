using System;
using ProtoBuf;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [LabelText("当有实体离开区域")]
    [ProtoContract]
    public partial class ConfigExitZoneEventTrigger : ConfigSceneGroupTrigger<ExitZoneEvent>
    {

    }
}