using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 以目标为原点
    /// </summary>
    public class ConfigBornByTarget: ConfigBornType
    {
        public override Vector3 ResolvePos(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            if (target is Unit u)
            {
                return u.Position + u.Rotation*PositionOffset;
            }
            return PositionOffset;
        }

        public override Quaternion ResolveRot(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            if (target is Unit u)
            {
                return Quaternion.Euler(u.Rotation.eulerAngles+RotationOffset);
            }
            return Quaternion.Euler(RotationOffset);
        }
    }
}