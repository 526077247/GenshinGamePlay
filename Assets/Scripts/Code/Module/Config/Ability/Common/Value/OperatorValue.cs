using ProtoBuf;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 操作值
    /// </summary>
    [ProtoContract]
    public partial class OperatorValue: BaseValue
    {
        [ProtoMember(1)][NotNull]
        public BaseValue Left;
        [ProtoMember(2)]
        public LogicMode Op;
        [ProtoMember(3)][NotNull][ShowIf("@Op != LogicMode.Default")]
        public BaseValue Right;


        public override float Resolve(Entity entity,ActorAbility ability)
        {
            switch (Op)
            {
                case LogicMode.Add:
                    return Left.Resolve(entity,ability) + Right.Resolve(entity,ability);
                case LogicMode.Red:
                    return Left.Resolve(entity,ability) - Right.Resolve(entity,ability);
                case LogicMode.Mul:
                    return Left.Resolve(entity,ability) * Right.Resolve(entity,ability);
                case LogicMode.Div:
                    return Left.Resolve(entity,ability) / Right.Resolve(entity,ability);
                case LogicMode.Rem:
                    if (Right.Resolve(entity,ability) == 0) return Left.Resolve(entity,ability);
                    return Left.Resolve(entity,ability) % Right.Resolve(entity,ability);
                case LogicMode.Pow:
                    return (int) Mathf.Pow(Left.Resolve(entity,ability), Right.Resolve(entity,ability));
                case LogicMode.Default:
                    return Left.Resolve(entity,ability);
            }

            return 0;
        }
    }
}