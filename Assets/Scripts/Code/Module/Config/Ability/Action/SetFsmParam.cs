namespace TaoTie
{
    public abstract class SetFsmParam<T> : ConfigAbilityAction where T: unmanaged
    {
        public string Key;
        public T Value;

        protected override void Execute(Entity applier, ActorAbility ability, ActorModifier modifier, Entity aim)
        {
            FsmComponent fc = aim.GetComponent<FsmComponent>();
            if (fc != null)
            {
                SetData(fc);
            }
        }

        protected abstract void SetData(FsmComponent fsmComponent);
    }
}