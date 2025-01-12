using Nino.Core;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ZeroValue: BaseValue
    {
        public override float Resolve(Entity entity, ActorAbility ability)
        {
            return 0;
        }
    }
}