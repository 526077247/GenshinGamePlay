using Nino.Core;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoType(false)]
    public abstract partial class ConfigBaseAttackPattern
    {
        [NinoMember(1)]
        public CheckHitLayerType CheckHitLayerType;

        [NinoMember(2)]
        [ShowIf("@" + nameof(CheckHitLayerType) + "!=TaoTie." + nameof(TaoTie.CheckHitLayerType) + "." +
                nameof(CheckHitLayerType.OnlyHitBox))]
        public ConfigHitScene HitScene;


        public abstract int ResolveHit(Entity applier, ActorAbility ability, ActorModifier modifier,
            Entity target, EntityType[] filter, out HitInfo[] hitInfos);

    }
}