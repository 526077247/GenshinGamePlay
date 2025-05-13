using Nino.Core;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
    public abstract partial class BaseVector3
    {
        public abstract Vector3 Resolve(Entity entity, ActorAbility ability);
    }
}