using Nino.Core;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigBornByAttachPoint : ConfigBornType
    {
        [NinoMember(5)] public string AttachPointName;
        [NinoMember(6)] public AttachPointTargetType AttachPointTargetType;

        public override Vector3 ResolvePos(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            var entity = AbilityHelper.ResolveTarget(actor, ability, modifier, target, AttachPointTargetType);
            var goh = entity?.GetComponent<GameObjectHolderComponent>();
            if (goh != null)
            {
                var trans = goh.GetCollectorObj<Transform>(AttachPointName);
                if (trans != null)
                {
                    return trans.position + trans.rotation * PositionOffset.Resolve(actor, ability);
                }
            }

            return PositionOffset.Resolve(actor, ability);
        }

        public override Quaternion ResolveRot(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            var goh = target.GetComponent<GameObjectHolderComponent>();
            if (goh != null)
            {
                var trans = goh.GetCollectorObj<Transform>(AttachPointName);
                if (trans != null)
                {
                    return Quaternion.Euler(trans.eulerAngles + RotationOffset.Resolve(actor, ability));
                }
            }

            return Quaternion.Euler(RotationOffset.Resolve(actor, ability));
        }

        public override async ETTask AfterBorn(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target,
            Entity bornEntity)
        {
            var goh = target.GetComponent<GameObjectHolderComponent>();
            if (goh != null)
            {
                await goh.WaitLoadGameObjectOver();
                if(goh.IsDispose) return;
                var trans = goh.GetCollectorObj<Transform>(AttachPointName);
                if (trans != null)
                {
                    var goh2 = bornEntity.GetComponent<GameObjectHolderComponent>();
                    if (goh2 != null)
                    {
                        await goh2.WaitLoadGameObjectOver();
                        if (!goh.IsDispose && !goh2.IsDispose)//防止创建回来父节点已经被销毁
                        {
                            goh2.EntityView.SetParent(trans);
                        }
                    }
                }
            }
        }
    }
}