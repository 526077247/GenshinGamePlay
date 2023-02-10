using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    public class SetFsmBool : SetFsmParam<bool>
    {
        protected override void SetData(FsmComponent fsmComponent)
        {
            fsmComponent.SetData(Key,Value);
        }
    }
}