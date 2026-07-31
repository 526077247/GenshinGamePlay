using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif

namespace TaoTie
{
    [LabelText("当剧情播放完成")]
    [ProtoContract]
    public partial class ConfigStoryPlayOverEvtTrigger : ConfigSceneGroupTrigger<StoryPlayOverEvt>
    {
        
    }
}