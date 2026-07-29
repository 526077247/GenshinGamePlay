using ProtoBuf;

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigAttackSphere: ConfigSimpleAttackPattern
    {
        [NotNull][ProtoMember(10, IsRequired = true)]
        public BaseValue Radius = new SingleValue(1);

        public override int ResolveHit(Entity applier, ActorAbility ability, ActorModifier modifier, Entity target,
            EntityType[] filter, out HitInfo[] hitInfos)
        {
            var pos = Born.ResolvePos(applier, ability, modifier, target);
            return PhysicsHelper.OverlapSphereNonAllocHitInfo(pos, Radius.Resolve(applier,ability), filter, CheckHitLayerType,
                out hitInfos);
        }
    }
}