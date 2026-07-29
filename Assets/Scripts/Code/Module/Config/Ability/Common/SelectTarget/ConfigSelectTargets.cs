using ProtoBuf;

namespace TaoTie
{
    /// <summary>
    /// 筛选
    /// </summary>
    [ProtoContract]
    [ProtoInclude(100, typeof(ConfigSelectTargetsByChildren))]
    [ProtoInclude(101, typeof(ConfigSelectTargetsByRange))]
    public abstract class ConfigSelectTargets
    {
        public abstract ListComponent<Entity> ResolveTargets(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target);
    }
}