namespace TaoTie
{
    /// <summary>
    /// 获取属性
    /// </summary>
    public class NumericValue: BaseValue
    {
        public int Key;
        public override float Resolve(Entity entity)
        {
            var numc = entity.GetComponent<NumericComponent>();
            if (numc != null)
            {
                return numc.GetAsFloat(Key);
            }
            return 0;
        }
    }
}