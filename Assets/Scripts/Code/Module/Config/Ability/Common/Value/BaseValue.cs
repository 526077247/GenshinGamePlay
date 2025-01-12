using Nino.Core;

namespace TaoTie
{
    /// <summary>
    /// 值
    /// </summary>
    [NinoType(false)]
    public abstract class BaseValue
    {
        public abstract float Resolve(Entity entity,ActorAbility ability);
    }
}