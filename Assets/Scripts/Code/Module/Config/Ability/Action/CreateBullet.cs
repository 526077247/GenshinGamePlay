using ProtoBuf;
using UnityEngine;

namespace TaoTie
{
    [ProtoContract]
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