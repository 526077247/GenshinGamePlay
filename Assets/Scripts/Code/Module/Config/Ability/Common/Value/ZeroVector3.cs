using ProtoBuf;
using UnityEngine;

namespace TaoTie
{
    [ProtoContract]
    public partial class ZeroVector3: BaseVector3
    {
        public override Vector3 Resolve(Entity entity, ActorAbility ability)
        {
            return Vector3.zero;
        }
    }
}