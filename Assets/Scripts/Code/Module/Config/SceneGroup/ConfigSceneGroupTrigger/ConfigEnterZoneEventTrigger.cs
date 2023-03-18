using System;
using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [LabelText("当有实体进入区域")]
    [NinoSerialize]
    public partial class ConfigEnterZoneEventTrigger : ConfigSceneGroupTrigger<EnterZoneEvent>
    {

    }
}