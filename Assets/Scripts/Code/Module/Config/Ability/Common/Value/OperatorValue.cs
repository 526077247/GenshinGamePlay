using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif
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


        /// <summary>
        /// 四则 / 取余 / 乘方。
        ///
        /// <b>modifier 必须原样下传给两个子节点</b>：整棵表达式树里只有叶子知道
        /// 自己要不要读上下文（<see cref="TagStackValue"/> 要，常量不要），
        /// 中间节点一旦吞掉参数，子树就会静默退化成"无上下文"，
        /// 表现是"配了层数倍率但伤害不变" —— 比报错难查得多。
        /// </summary>
        public override float Resolve(Entity entity,ActorAbility ability,ActorModifier modifier = null)
        {
            switch (Op)
            {
                case LogicMode.Add:
                    return Left.Resolve(entity,ability,modifier) + Right.Resolve(entity,ability,modifier);
                case LogicMode.Red:
                    return Left.Resolve(entity,ability,modifier) - Right.Resolve(entity,ability,modifier);
                case LogicMode.Mul:
                    return Left.Resolve(entity,ability,modifier) * Right.Resolve(entity,ability,modifier);
                case LogicMode.Div:
                    return Left.Resolve(entity,ability,modifier) / Right.Resolve(entity,ability,modifier);
                case LogicMode.Rem:
                    if (Right.Resolve(entity,ability,modifier) == 0) return Left.Resolve(entity,ability,modifier);
                    return Left.Resolve(entity,ability,modifier) % Right.Resolve(entity,ability,modifier);
                case LogicMode.Pow:
                    return (int) Mathf.Pow(Left.Resolve(entity,ability,modifier), Right.Resolve(entity,ability,modifier));
                case LogicMode.Default:
                    return Left.Resolve(entity,ability,modifier);
            }

            return 0;
        }
    }
}