using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    public partial class SetFsmBool : SetFsmParam<bool>
    {
        protected override void SetData(FsmComponent fsmComponent)
        {
            fsmComponent.SetData(Key,Value);
        }
    }
}