using ProtoBuf;

namespace TaoTie
{
    [ProtoContract]
    public partial class SetFsmFloat : SetFsmParam<float>
    {
        protected override void SetData(FsmComponent fsmComponent)
        {
            fsmComponent.SetData(Key,Value);
        }
    }
}