using Nino.Core;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigAttackTarget: ConfigBaseAttackPattern
    {

        public override int ResolveHit(Entity applier, ActorAbility ability, ActorModifier modifier, Entity target, EntityType[] filter,
            out HitInfo[] hitInfos)
        {
            var vcf = applier.GetComponent<UnitModelComponent>();
            var tbc = vcf?.EntityView?.GetComponentInChildren<TriggerBoxComponent>();
            var vct = target.GetComponent<UnitModelComponent>();
            var colliders = vct?.EntityView?.GetComponentsInChildren<Collider>();
            return PhysicsHelper.OverlapColliderNonAllocHitInfo(tbc, colliders, filter, CheckHitLayerType,
                out hitInfos);
        }
    }
}