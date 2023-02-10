using Nino.Serialization;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public class DynamicVector3
    {
        [NinoMember(1)][NotNull]
        public BaseValue X;
        [NinoMember(2)][NotNull]
        public BaseValue Y;
        [NinoMember(3)][NotNull]
        public BaseValue Z;
        
        public Vector3 Resolve(Entity entity, ActorAbility ability)
        {
            return new Vector3(X.Resolve(entity, ability), Y.Resolve(entity, ability), Z.Resolve(entity, ability));
        }
    }
}