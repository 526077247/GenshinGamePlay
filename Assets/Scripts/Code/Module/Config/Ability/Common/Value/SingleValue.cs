namespace TaoTie
{
    /// <summary>
    /// 固定值
    /// </summary>
    public class SingleValue: BaseValue
    {
        public float Value;
        public override float Resolve(Entity entity)
        {
            return Value;
        }
    }
}