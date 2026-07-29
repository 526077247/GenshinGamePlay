using ProtoBuf;
using UnityEngine;

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigAttackBox: ConfigSimpleAttackPattern
    {
        [ProtoMember(10, IsRequired = true)][NotNull]
        public DynamicVector3 Size = new DynamicVector3();

        public override int ResolveHit(Entity applier, ActorAbility ability, ActorModifier modifier, Entity target,
            EntityType[] filter, out HitInfo[] hitInfos)
        {
            var pos = Born.ResolvePos(applier, ability, modifier, target);
            var rot = Born.ResolveRot(applier, ability, modifier, target);
            return PhysicsHelper.OverlapBoxNonAllocHitInfo(pos, Size.Resolve(applier, ability) * 0.5f, rot, filter, CheckHitLayerType,
                out hitInfos);
        }
    }
}