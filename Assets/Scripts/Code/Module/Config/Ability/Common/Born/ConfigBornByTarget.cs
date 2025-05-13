using Nino.Core;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 以目标为原点
    /// </summary>
    [NinoType(false)]
    public partial class ConfigBornByTarget: ConfigBornType
    {
        public override Vector3 ResolvePos(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            if (target is SceneEntity u)
            {
                return u.Position + u.Rotation * PositionOffset.Resolve(target, ability);
            }

            return PositionOffset.Resolve(target, ability);
        }

        public override Quaternion ResolveRot(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            if (target is SceneEntity u)
            {
                return Quaternion.Euler(u.Rotation.eulerAngles + RotationOffset.Resolve(target, ability));
            }

            return Quaternion.Euler(RotationOffset.Resolve(target, ability));
        }
    }
}