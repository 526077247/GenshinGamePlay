using Nino.Core;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
    public partial class CreateBullet: CreateGadget
    {
        protected override void Execute(Entity actionExecuter, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            var res = CreateGadgetInner(actionExecuter, ability, modifier, target);
            if (res != null)
            {
                res.AddComponent<BulletComponent>();
            }
        }
    }
}