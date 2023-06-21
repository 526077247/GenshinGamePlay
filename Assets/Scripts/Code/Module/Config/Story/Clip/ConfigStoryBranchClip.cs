using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoSerialize]
    public class ConfigStoryBranchClipItem
    {
        [NinoMember(1)]
        public ConfigStoryText Text;
        [NinoMember(2)]
        public ConfigStoryClip Clip;
    }
    
    [LabelText("选择分支执行")][NinoSerialize]
    public class ConfigStoryBranchClip: ConfigStoryClip
    {
        [NinoMember(10)]
        public ConfigStoryBranchClipItem[] Branchs;

        public override ETTask Process()
        {
            throw new System.NotImplementedException();
        }
    }
}