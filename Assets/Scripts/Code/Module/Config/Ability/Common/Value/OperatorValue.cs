namespace TaoTie
{
    /// <summary>
    /// 操作值
    /// </summary>
    public class OperatorValue: BaseValue
    {
        public BaseValue Left;
        public Operator Op;
        public BaseValue Right;
        public enum Operator
        {
            Add,
            Sub,
            Mul,
            Div
        }

        public override float Resolve(Entity entity)
        {
            switch (Op)
            {
                case Operator.Add:
                    return Left.Resolve(entity) + Right.Resolve(entity);
                case Operator.Sub:
                    return Left.Resolve(entity) - Right.Resolve(entity);
                case Operator.Mul:
                    return Left.Resolve(entity) * Right.Resolve(entity);
                case Operator.Div:
                    return Left.Resolve(entity) / Right.Resolve(entity);
            }

            return 0;
        }
    }
}