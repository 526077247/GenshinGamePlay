using Nino.Core;

namespace TaoTie
{
    /// <summary>
    /// 公式运算值
    /// </summary>
    [NinoType(false)]
    public partial class FormulaValue: BaseValue
    {
        [NinoMember(1)]
        public string Formula;
        public override float Resolve(Entity entity, ActorAbility ability)
        {
            var numc = entity.GetComponent<NumericComponent>();
            if (numc != null)
            {
                return FormulaStringFx.Get(Formula).GetData(numc,ability);
            }
            Log.Error($"获取{Formula}时，未找到NumericComponent组件");
            return 0;
        }
    }
}