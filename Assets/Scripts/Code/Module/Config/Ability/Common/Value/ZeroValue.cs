using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ZeroValue: BaseValue
    {
        public override float Resolve(Entity entity, ActorAbility ability)
        {
            return 0;
        }
    }
}