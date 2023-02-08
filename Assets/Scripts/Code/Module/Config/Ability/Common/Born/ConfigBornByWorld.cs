using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 世界坐标为原点
    /// </summary>
    public class ConfigBornByWorld: ConfigBornType
    {
        public override Vector3 ResolvePos(Entity actor, ActorAbility ability, ActorModifier modifier, Entity aim)
        {
            return PositionOffset;
        }

        public override Quaternion ResolveRot(Entity actor, ActorAbility ability, ActorModifier modifier, Entity aim)
        {
            return Quaternion.Euler(RotationOffset);
        }
    }
}