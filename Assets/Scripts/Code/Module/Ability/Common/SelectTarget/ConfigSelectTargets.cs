using Nino.Serialization;

namespace TaoTie
{
    /// <summary>
    /// 筛选
    /// </summary>
    [NinoSerialize]
    public abstract class ConfigSelectTargets
    {
        public abstract Entity[] ResolveTargets(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target);
    }
}