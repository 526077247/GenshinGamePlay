using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [LabelText("当寻路单位抵达某个需要广播抵达事件的位置")]
    [NinoSerialize]
    public class ConfigPlatformReachPointEvtTrigger : ConfigSceneGroupTrigger<PlatformReachPointEvt>
    {
        
    }
}