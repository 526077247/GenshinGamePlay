using ProtoBuf;
using UnityEngine;

namespace TaoTie
{
    [ProtoContract]
    [ProtoInclude(200, typeof(DynamicVector3))]
    [ProtoInclude(201, typeof(ZeroVector3))]
    public abstract partial class BaseVector3
    {
        public abstract Vector3 Resolve(Entity entity, ActorAbility ability);
    }
}