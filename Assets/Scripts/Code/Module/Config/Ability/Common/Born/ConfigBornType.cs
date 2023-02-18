using Nino.Serialization;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 出生点
    /// </summary>
    public abstract partial class ConfigBornType
    {
        [NinoMember(1)][NotNull]
        public DynamicVector3 PositionOffset;
        [NinoMember(2)][NotNull]
        public DynamicVector3 RotationOffset;

        public abstract Vector3 ResolvePos(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target);

        public abstract Quaternion ResolveRot(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target);
    }
}