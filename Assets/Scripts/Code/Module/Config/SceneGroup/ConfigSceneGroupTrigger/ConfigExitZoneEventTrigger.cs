using System;
using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [LabelText("当有实体离开区域")]
    [NinoSerialize]
    public partial class ConfigExitZoneEventTrigger : ConfigSceneGroupTrigger<ExitZoneEvent>
    {

    }
}