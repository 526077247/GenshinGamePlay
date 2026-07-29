using ProtoBuf;

namespace TaoTie
{
    [ProtoContract]
    public partial class SetFsmBool : SetFsmParam<bool>
    {
        protected override void SetData(FsmComponent fsmComponent)
        {
            fsmComponent.SetData(Key,Value);
        }
    }
}