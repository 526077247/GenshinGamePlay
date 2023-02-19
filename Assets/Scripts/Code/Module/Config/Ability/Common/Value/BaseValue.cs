using Nino.Serialization;

namespace TaoTie
{
    /// <summary>
    /// 值
    /// </summary>
    public abstract class BaseValue
    {
        public abstract float Resolve(Entity entity,ActorAbility ability);
    }
}