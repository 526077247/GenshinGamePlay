using Nino.Serialization;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 以目标为原点
    /// </summary>
    [NinoSerialize]
    public partial class ConfigBornByTarget: ConfigBornType
    {
        public override Vector3 ResolvePos(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            if (target is Unit u)
            {
                return u.Position + u.Rotation*PositionOffset.Resolve(actor,ability);
            }
            return PositionOffset.Resolve(actor,ability);
        }

        public override Quaternion ResolveRot(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            if (target is Unit u)
            {
                return Quaternion.Euler(u.Rotation.eulerAngles+RotationOffset.Resolve(actor,ability));
            }
            return Quaternion.Euler(RotationOffset.Resolve(actor,ability));
        }
    }
}