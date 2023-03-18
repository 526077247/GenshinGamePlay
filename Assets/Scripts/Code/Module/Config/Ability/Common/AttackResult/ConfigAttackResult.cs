using Nino.Serialization;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 攻击结果
    /// </summary>
    public abstract partial class ConfigAttackResult
    {
        //[NinoMember(1)][NotNull]
        //public DynamicVector3 PositionOffset;
        //[NinoMember(2)][NotNull]
        //public DynamicVector3 RotationOffset;

        public abstract void ResolveAttackResult(AttackResult attackResult);
    }
}