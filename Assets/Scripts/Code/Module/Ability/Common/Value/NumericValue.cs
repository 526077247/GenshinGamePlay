using Nino.Serialization;

namespace TaoTie
{
    /// <summary>
    /// 属性值
    /// </summary>
    [NinoSerialize]
    public partial class NumericValue: BaseValue
    {
        [NinoMember(1)]
        public int Key;
        public override float Resolve(Entity entity,ActorAbility ability)
        {
            var numc = entity.GetComponent<NumericComponent>();
            if (numc != null)
            {
                return numc.GetAsFloat(Key);
            }
            Log.Error($"获取{Key}时，未找到NumericComponent组件");
            return 0;
        }
    }
}