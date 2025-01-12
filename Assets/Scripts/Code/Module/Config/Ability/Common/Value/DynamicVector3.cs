using Nino.Core;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
    public partial class DynamicVector3
    {
        [NinoMember(1)][NotNull]
        public BaseValue X = new ZeroValue();
        [NinoMember(2)][NotNull]
        public BaseValue Y = new ZeroValue();
        [NinoMember(3)][NotNull]
        public BaseValue Z = new ZeroValue();
        
        public Vector3 Resolve(Entity entity, ActorAbility ability)
        {
            return new Vector3(X.Resolve(entity, ability), Y.Resolve(entity, ability), Z.Resolve(entity, ability));
        }
    }
}