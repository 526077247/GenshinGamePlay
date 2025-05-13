using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 出生点
    /// </summary>
    [NinoType(false)]
    public abstract partial class ConfigBornType
    {
        [NinoMember(1)][NotNull][LabelText("坐标偏移")]
        public BaseVector3 PositionOffset = new ZeroVector3();
        [NinoMember(2)][NotNull][LabelText("方向偏移")]
        public BaseVector3 RotationOffset = new ZeroVector3();

        public abstract Vector3 ResolvePos(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target);

        public abstract Quaternion ResolveRot(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target);

        public virtual async ETTask AfterBorn(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target, Entity bornEntity)
        {
            await ETTask.CompletedTask;
        }
    }
}