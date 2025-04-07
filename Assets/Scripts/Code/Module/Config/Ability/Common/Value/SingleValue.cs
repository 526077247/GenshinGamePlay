using Nino.Core;

namespace TaoTie
{
    /// <summary>
    /// 固定值
    /// </summary>
    [NinoType(false)]
    public partial class SingleValue: BaseValue
    {
        public SingleValue()
        {
            Value = 0;
        }
        public SingleValue(float val)
        {
            Value = val;
        }
        [NinoMember(1)]
        public float Value;
        public override float Resolve(Entity entity,ActorAbility ability)
        {
            return Value;
        }
    }
}