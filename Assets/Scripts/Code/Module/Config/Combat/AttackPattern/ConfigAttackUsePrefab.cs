using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    public class ConfigAttackUsePrefab: ConfigBaseAttackPattern
    {
        [NinoMember(1)]
        public string PrefabPathName;

        public override int ResolveHit(Entity applier, ActorAbility ability, ActorModifier modifier, Entity target, EntityType[] filter,
            out HitInfo[] hitInfos)
        {
            throw new System.NotImplementedException();
        }
    }
}