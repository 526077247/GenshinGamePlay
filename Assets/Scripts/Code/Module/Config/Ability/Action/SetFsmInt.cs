using ProtoBuf;

namespace TaoTie
{
    [ProtoContract]
    public partial class SetFsmInt : SetFsmParam<int>
    {
        protected override void SetData(FsmComponent fsmComponent)
        {
            fsmComponent.SetData(Key,Value);
        }
    }
}