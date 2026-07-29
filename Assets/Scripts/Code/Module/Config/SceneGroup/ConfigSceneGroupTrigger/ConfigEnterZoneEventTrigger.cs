using System;
using ProtoBuf;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [LabelText("当有实体进入区域")]
    [ProtoContract]
    public partial class ConfigEnterZoneEventTrigger : ConfigSceneGroupTrigger<EnterZoneEvent>
    {

    }
}