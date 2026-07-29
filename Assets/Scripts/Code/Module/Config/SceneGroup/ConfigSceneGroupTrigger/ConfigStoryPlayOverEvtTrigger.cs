using ProtoBuf;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [LabelText("当剧情播放完成")]
    [ProtoContract]
    public partial class ConfigStoryPlayOverEvtTrigger : ConfigSceneGroupTrigger<StoryPlayOverEvt>
    {
        
    }
}