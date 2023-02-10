using UnityEngine;

namespace TaoTie
{
    public class ConfigAttackBox: ConfigSimpleAttackPattern
    {
        public Vector3 Size;

        public override int ResolveHit(Entity applier, ActorAbility ability, ActorModifier modifier, Entity target,
            EntityType[] filter, out HitInfo[] hitInfos)
        {
            var pos = Born.ResolvePos(applier, ability, modifier, target);
            var rot = Born.ResolveRot(applier, ability, modifier, target);
            return PhysicsHelper.OverlapBoxNonAllocHitInfo(pos, Size * 0.5f, rot, filter, CheckHitLayerType,
                out hitInfos);
        }
    }
}