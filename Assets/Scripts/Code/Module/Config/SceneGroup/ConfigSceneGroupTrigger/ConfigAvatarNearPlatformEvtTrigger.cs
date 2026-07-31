using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif

namespace TaoTie
{
    [LabelText("当玩家靠近寻路单位")]
    [ProtoContract]
    public partial class ConfigAvatarNearPlatformEvtTrigger : ConfigSceneGroupTrigger<AvatarNearPlatformEvt>
    {
        
    }
}