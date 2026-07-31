using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif

namespace TaoTie
{
    [LabelText("当寻路单位抵达某个需要广播抵达事件的位置")]
    [ProtoContract]
    public class ConfigPlatformReachPointEvtTrigger : ConfigSceneGroupTrigger<PlatformReachPointEvt>
    {
        
    }
}