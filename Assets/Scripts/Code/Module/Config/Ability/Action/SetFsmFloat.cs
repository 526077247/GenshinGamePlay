namespace TaoTie
{
    public class SetFsmFloat : SetFsmParam<float>
    {
        protected override void SetData(FsmComponent fsmComponent)
        {
            fsmComponent.SetData(Key,Value);
        }
    }
}