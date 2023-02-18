using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigEntityPoint
    {
        [NinoMember(1)]
        public string[] HitPoints;
        [NinoMember(2)]
        public string[] SelectedPoints;
    }
}