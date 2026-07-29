using ProtoBuf;
using UnityEngine;

namespace TaoTie
{
    [ProtoContract]
    public partial class DynamicVector3: BaseVector3
    {
        [ProtoMember(1, IsRequired = true)][NotNull]
        public BaseValue X = new SingleValue();
        [ProtoMember(2, IsRequired = true)][NotNull]
        public BaseValue Y = new SingleValue();
        [ProtoMember(3, IsRequired = true)][NotNull]
        public BaseValue Z = new SingleValue();
        
        public override Vector3 Resolve(Entity entity, ActorAbility ability)
        {
            return new Vector3(X.Resolve(entity, ability), Y.Resolve(entity, ability), Z.Resolve(entity, ability));
        }
    }
}