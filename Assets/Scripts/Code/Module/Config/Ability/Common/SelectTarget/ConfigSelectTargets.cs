namespace TaoTie
{
    /// <summary>
    /// 筛选
    /// </summary>
    public abstract class ConfigSelectTargets
    {
        public abstract Entity[] ResolveTargets(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target);
    }
}