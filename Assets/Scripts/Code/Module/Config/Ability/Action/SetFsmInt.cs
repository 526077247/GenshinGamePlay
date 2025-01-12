using Nino.Core;

namespace TaoTie
{
    [NinoType(false)]
    public partial class SetFsmInt : SetFsmParam<int>
    {
        protected override void SetData(FsmComponent fsmComponent)
        {
            fsmComponent.SetData(Key,Value);
        }
    }
}