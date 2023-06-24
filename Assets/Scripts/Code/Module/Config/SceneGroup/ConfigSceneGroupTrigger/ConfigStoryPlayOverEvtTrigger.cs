using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [LabelText("当剧情播放完成")]
    [NinoSerialize()]
    public partial class ConfigStoryPlayOverEvtTrigger : ConfigSceneGroupTrigger<StoryPlayOverEvt>
    {
        
    }
}