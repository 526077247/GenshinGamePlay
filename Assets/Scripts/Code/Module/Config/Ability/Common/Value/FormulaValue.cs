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
            throw new System.NotImplementedException();
        }
    }
}