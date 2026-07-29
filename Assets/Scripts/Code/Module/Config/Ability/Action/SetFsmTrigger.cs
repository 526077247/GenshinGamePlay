using ProtoBuf;

namespace TaoTie
{
    [ProtoContract]
    public partial class SetFsmTrigger : SetFsmParam<bool>
    {
        protected override void SetData(FsmComponent fsmComponent)
        {
            fsmComponent.SetData(Key,Value);
        }
    }
}