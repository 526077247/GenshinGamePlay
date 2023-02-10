namespace TaoTie
{
    public abstract class ConfigBaseAttackPattern
    {
        public CheckHitLayerType CheckHitLayerType;
        public ConfigHitScene HitScene;
        [NotNull]
        public ConfigBornType Born;

        public abstract int ResolveHit(Entity applier, ActorAbility ability, ActorModifier modifier,
            Entity target, EntityType[] filter, out HitInfo[] hitInfos);

    }
}