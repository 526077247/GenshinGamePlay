using ProtoBuf;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigHitImpulse
    {
        [ProtoMember(1)][LabelText("击打力度等级")]
        public HitLevel HitLevel;
        [ProtoMember(2, IsRequired = true)]
        public BaseValue HitImpulseX = new SingleValue();
        [ProtoMember(3, IsRequired = true)]
        public BaseValue HitImpulseY = new SingleValue();
    }
}