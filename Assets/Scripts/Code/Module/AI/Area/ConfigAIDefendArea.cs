using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigAIDefendArea
    {
        [NinoMember(1)]
        private bool enable;
        [NinoMember(2)]
        private float defendRangeRawNum; 
    }
}