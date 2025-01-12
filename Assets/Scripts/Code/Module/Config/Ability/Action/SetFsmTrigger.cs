using Nino.Core;

namespace TaoTie
{
    [NinoType(false)]
    public partial class SetFsmTrigger : SetFsmParam<bool>
    {
        protected override void SetData(FsmComponent fsmComponent)
        {
            fsmComponent.SetData(Key,Value);
        }
    }
}