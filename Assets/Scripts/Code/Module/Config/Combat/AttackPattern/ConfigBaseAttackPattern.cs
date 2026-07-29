using ProtoBuf;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [ProtoContract]
    [ProtoInclude(100, typeof(ConfigAttackColliderBox))]
    [ProtoInclude(101, typeof(ConfigSimpleAttackPattern))]
    public abstract partial class ConfigBaseAttackPattern
    {
        [ProtoMember(1)]
        public CheckHitLayerType CheckHitLayerType;

        [ProtoMember(2)]
        [ShowIf("@" + nameof(CheckHitLayerType) + "!=TaoTie." + nameof(TaoTie.CheckHitLayerType) + "." +
                nameof(CheckHitLayerType.OnlyHitBox))]
        public ConfigHitScene HitScene;


        public abstract int ResolveHit(Entity applier, ActorAbility ability, ActorModifier modifier,
            Entity target, EntityType[] filter, out HitInfo[] hitInfos);

    }
}