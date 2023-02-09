namespace TaoTie
{
    public class ConfigAttackUsePrefab: ConfigBaseAttackPattern
    {
        public string PrefabPathName;

        public override int ResolveHit(Entity applier, ActorAbility ability, ActorModifier modifier, Entity target, EntityType[] filter,
            out HitInfo[] hitInfos)
        {
            throw new System.NotImplementedException();
        }
    }
}