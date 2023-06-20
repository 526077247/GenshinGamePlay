using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    public abstract class StoryClipConfig
    {
        [NinoMember(1)][LabelText("策划备注")]
        public string Remarks;
    }
}