namespace TaoTie
{
    public enum AbilityActionTarget
    {
        Self,
        Caster,
        Enemy,
        SelfAttackTarget,
        Applier,    // modifier applier
        CurTeamAvatars,
        CurLocalAvatar,
        Team,
        Owner,
    }
    public abstract class ConfigAbilityAction
    {
        public AbilityActionTarget ActionTarget;

        public abstract void DoExecute(ActorAbility actorAbility, Entity other);
    }
}