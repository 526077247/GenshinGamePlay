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
                var bullet = res.AddComponent<BulletComponent>();
                // 记录「谁打出的这颗子弹」。取执行者而不是 ability.ApplierID：
                // 子弹技能是挂在子弹自己身上的，ApplierID 拿到的是子弹本身，不是射手。
                // 执行者本身也是子弹时（子弹生子弹）继承上一级 OwnerId，避免链式生成后来源断掉。
                var executerBullet = actionExecuter.GetComponent<BulletComponent>();
                bullet.OwnerId = executerBullet != null && executerBullet.OwnerId != 0
                    ? executerBullet.OwnerId
                    : actionExecuter.Id;
            }
        }
    }
}
