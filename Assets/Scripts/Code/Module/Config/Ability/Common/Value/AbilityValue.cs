using Nino.Core;

namespace TaoTie
{
    /// <summary>
    /// 取Ability值
    /// </summary>
    [NinoType(false)]
    public partial class AbilityValue: BaseValue
    {
        [NinoMember(1)]
        public string Key;
        public override float Resolve(Entity entity,ActorAbility ability)
        {
            
            return 0;
        }
    }
}