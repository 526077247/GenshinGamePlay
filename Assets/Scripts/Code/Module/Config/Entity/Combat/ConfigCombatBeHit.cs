using ProtoBuf;

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigCombatBeHit
    {
        [ProtoMember(1)]
        public string HitBloodEffect;
        [ProtoMember(2)]
        public bool MuteAllHit; 
        [ProtoMember(3)]
        public bool MuteAllHitEffect;
        [ProtoMember(4)]
        public bool MuteAllHitText;
    }
}