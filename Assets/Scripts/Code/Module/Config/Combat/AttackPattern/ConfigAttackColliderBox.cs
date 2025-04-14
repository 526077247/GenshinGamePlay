using Nino.Core;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigAttackColliderBox: ConfigBaseAttackPattern
    {

        public override int ResolveHit(Entity applier, ActorAbility ability, ActorModifier modifier, Entity target, EntityType[] filter,
            out HitInfo[] hitInfos)
        {
            var vc = target.GetComponent<UnitModelComponent>();
            var tbs = vc?.EntityView?.GetComponentInChildren<ColliderBoxComponent>();
            return PhysicsHelper.OverlapColliderNonAllocHitInfo(tbs, filter, CheckHitLayerType,
                out hitInfos);
        }
    }
}