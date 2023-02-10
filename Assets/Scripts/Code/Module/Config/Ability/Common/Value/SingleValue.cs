using Nino.Serialization;

namespace TaoTie
{
    /// <summary>
    /// 固定值
    /// </summary>
    [NinoSerialize]
    public partial class SingleValue: BaseValue
    {
        [NinoMember(1)]
        public float Value;
        public override float Resolve(Entity entity,ActorAbility ability)
        {
            return Value;
        }
    }
}