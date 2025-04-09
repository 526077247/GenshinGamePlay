using Nino.Core;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 出生点
    /// </summary>
    [NinoType(false)]
    public abstract partial class ConfigBornType
    {
        [NinoMember(1)][NotNull]
        public DynamicVector3 PositionOffset = new DynamicVector3();
        [NinoMember(2)][NotNull]
        public DynamicVector3 RotationOffset = new DynamicVector3();

        public abstract Vector3 ResolvePos(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target);

        public abstract Quaternion ResolveRot(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target);

        public virtual async ETTask AfterBorn(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target, Entity bornEntity)
        {
            await ETTask.CompletedTask;
        }
    }
}