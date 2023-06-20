using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoSerialize]
    public class BranchStoryClipConfigItem
    {
        [NinoMember(1)]
        public StoryText Text;
        [NinoMember(2)]
        public StoryClipConfig Clip;
    }
    
    [LabelText("选择分支执行")][NinoSerialize]
    public class BranchStoryClipConfig: StoryClipConfig
    {
        [NinoMember(10)]
        public BranchStoryClipConfigItem[] Branchs;
    }
}