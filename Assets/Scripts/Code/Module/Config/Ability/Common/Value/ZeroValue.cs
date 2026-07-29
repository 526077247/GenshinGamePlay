using ProtoBuf;

namespace TaoTie
{
    [ProtoContract]
    public partial class ZeroValue: BaseValue
    {
        public override float Resolve(Entity entity, ActorAbility ability)
        {
            return 0;
        }
    }
}