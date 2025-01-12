using Nino.Core;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoType(false)]
    public abstract partial class ConfigStoryClip
    {
#if UNITY_EDITOR
        [LabelText("策划备注")]
        public string Remarks;
#endif

        public abstract ETTask Process(StorySystem storySystem);
    }
}