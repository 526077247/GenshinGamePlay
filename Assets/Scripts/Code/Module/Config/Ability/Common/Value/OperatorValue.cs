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

        public override float Resolve(Entity entity,ActorAbility ability)
        {
            switch (Op)
            {
                case Operator.Add:
                    return Left.Resolve(entity,ability) + Right.Resolve(entity,ability);
                case Operator.Sub:
                    return Left.Resolve(entity,ability) - Right.Resolve(entity,ability);
                case Operator.Mul:
                    return Left.Resolve(entity,ability) * Right.Resolve(entity,ability);
                case Operator.Div:
                    return Left.Resolve(entity,ability) / Right.Resolve(entity,ability);
            }

            return 0;
        }
    }
}