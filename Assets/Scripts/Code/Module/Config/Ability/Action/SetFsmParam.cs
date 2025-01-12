using Nino.Core;

namespace TaoTie
{
    [NinoType(false)]
    public abstract class SetFsmParam<T> : ConfigAbilityAction where T: unmanaged
    {
        [NinoMember(10)]
        public string Key;
        [NinoMember(11)]
        public T Value;

        protected override void Execute(Entity applier, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            FsmComponent fc = target.GetComponent<FsmComponent>();
            if (fc != null)
            {
                SetData(fc);
            }
        }

        protected abstract void SetData(FsmComponent fsmComponent);
    }
}