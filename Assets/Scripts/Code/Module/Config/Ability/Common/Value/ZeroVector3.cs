using Nino.Core;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ZeroVector3: BaseVector3
    {
        public override Vector3 Resolve(Entity entity, ActorAbility ability)
        {
            return Vector3.zero;
        }
    }
}