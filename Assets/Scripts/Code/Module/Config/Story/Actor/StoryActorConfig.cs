using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    public abstract class StoryActorConfig
    {
        [NinoMember(1)][LabelText("策划备注")]
        public string Remarks;
        [NinoMember(2)]
        public int Id;
    }
}