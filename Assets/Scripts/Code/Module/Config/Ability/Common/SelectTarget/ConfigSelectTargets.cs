using Nino.Core;

namespace TaoTie
{
    /// <summary>
    /// 筛选
    /// </summary>
    [NinoType(false)]
    public abstract class ConfigSelectTargets
    {
        public abstract ListComponent<Entity> ResolveTargets(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target);
    }
}