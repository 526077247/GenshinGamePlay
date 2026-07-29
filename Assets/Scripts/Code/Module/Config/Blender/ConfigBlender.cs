using ProtoBuf;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigBlender
    {
        [ProtoMember(2, IsRequired = true)]
        public EasingFunction.Ease Ease = EasingFunction.Ease.Linear;

        [ProtoMember(1, IsRequired = true)][LabelText("过渡时间(ms)")]
        public int DeltaTime = 1000;
    }
}