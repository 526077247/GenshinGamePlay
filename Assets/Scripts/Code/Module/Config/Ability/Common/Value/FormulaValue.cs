namespace TaoTie
{
    /// <summary>
    /// 公式运算值
    /// </summary>
    public class FormulaValue: BaseValue
    {
        public string Formula;
        public override float Resolve(Entity entity)
        {
            var numc = entity.GetComponent<NumericComponent>();
            if (numc == null) return 0;
            return FormulaStringFx.Get(Formula).GetData(numc);
        }
    }
}