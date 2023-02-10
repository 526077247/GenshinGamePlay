using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    public class SetFsmFloat : SetFsmParam<float>
    {
        protected override void SetData(FsmComponent fsmComponent)
        {
            fsmComponent.SetData(Key,Value);
        }
    }
}