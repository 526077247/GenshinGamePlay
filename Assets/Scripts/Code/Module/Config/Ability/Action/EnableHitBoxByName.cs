using Nino.Core;

namespace TaoTie
{
    [NinoType(false)]
    public partial class EnableHitBoxByName: ConfigAbilityAction
    {
        [NinoMember(10)]
        public string[] HitBoxNames;
        [NinoMember(11)]
        public bool SetEnable;
        protected override void Execute(Entity applier, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            if (HitBoxNames != null)
            {
                GameObjectHolderComponent holderComponent = target.GetComponent<GameObjectHolderComponent>();
                for (int i = 0; i < HitBoxNames.Length; i++)
                {
                    holderComponent?.EnableHitBox(HitBoxNames[i], SetEnable).Coroutine();
                }
            }
        }
    }
}