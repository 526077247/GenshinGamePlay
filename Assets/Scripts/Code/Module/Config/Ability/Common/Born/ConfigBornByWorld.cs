using Nino.Serialization;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 世界坐标为原点
    /// </summary>
    [NinoSerialize]
    public class ConfigBornByWorld: ConfigBornType
    {
        public override Vector3 ResolvePos(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            return PositionOffset.Resolve(actor,ability);
        }

        public override Quaternion ResolveRot(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            return Quaternion.Euler(RotationOffset.Resolve(actor,ability));
        }
    }
}