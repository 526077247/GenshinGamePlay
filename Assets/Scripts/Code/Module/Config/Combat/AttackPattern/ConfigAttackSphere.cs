﻿using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigAttackSphere: ConfigSimpleAttackPattern
    {
        [NotNull][NinoMember(10)]
        public BaseValue Radius;

        public override int ResolveHit(Entity applier, ActorAbility ability, ActorModifier modifier, Entity target,
            EntityType[] filter, out HitInfo[] hitInfos)
        {
            var pos = Born.ResolvePos(applier, ability, modifier, target);
            return PhysicsHelper.OverlapSphereNonAllocHitInfo(pos, Radius.Resolve(applier,ability), filter, CheckHitLayerType,
                out hitInfos);
        }
    }
}