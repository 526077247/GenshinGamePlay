using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    public abstract partial class ConfigStoryClip
    {
        [NinoMember(1)][LabelText("策划备注")]
        public string Remarks;


        public abstract ETTask Process(StorySystem storySystem);
    }
}