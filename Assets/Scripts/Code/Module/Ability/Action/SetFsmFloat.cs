using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    public partial class SetFsmFloat : SetFsmParam<float>
    {
        protected override void SetData(FsmComponent fsmComponent)
        {
            fsmComponent.SetData(Key,Value);
        }
    }
}