namespace TaoTie
{
    public class SetFsmTrigger : SetFsmParam<bool>
    {
        protected override void SetData(FsmComponent fsmComponent)
        {
            fsmComponent.SetData(Key,Value);
        }
    }
}