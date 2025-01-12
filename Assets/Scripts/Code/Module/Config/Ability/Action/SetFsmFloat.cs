using Nino.Core;

namespace TaoTie
{
    [NinoType(false)]
    public partial class SetFsmFloat : SetFsmParam<float>
    {
        protected override void SetData(FsmComponent fsmComponent)
        {
            fsmComponent.SetData(Key,Value);
        }
    }
}