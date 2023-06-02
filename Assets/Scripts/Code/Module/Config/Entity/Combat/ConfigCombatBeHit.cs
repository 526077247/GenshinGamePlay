using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigCombatBeHit
    {
        [NinoMember(1)]
        public string HitBloodEffect;
        [NinoMember(2)]
        public bool MuteAllHit; 
        [NinoMember(3)]
        public bool MuteAllHitEffect;
        [NinoMember(4)]
        public bool MuteAllHitText;
    }
}