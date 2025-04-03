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
            var entity = AbilitySystem.ResolveTarget(actor, ability, modifier, target, AttachPointTargetType);
            var model = entity?.GetComponent<ModelComponent>();
            if (model != null)
            {
                var trans = model.GetCollectorObj<Transform>(AttachPointName);
                if (trans != null)
                {
                    return trans.position + trans.rotation * PositionOffset.Resolve(actor, ability);
                }
                else
                {
                    Log.Error("挂点不存在"+AttachPointName);
                }
            }

            return PositionOffset.Resolve(actor, ability);
        }

        public override Quaternion ResolveRot(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            var model = target.GetComponent<ModelComponent>();
            if (model != null)
            {
                var trans = model.GetCollectorObj<Transform>(AttachPointName);
                if (trans != null)
                {
                    return Quaternion.Euler(trans.eulerAngles + RotationOffset.Resolve(actor, ability));
                }
                else
                {
                    Log.Error("挂点不存在"+AttachPointName);
                }
            }

            return Quaternion.Euler(RotationOffset.Resolve(actor, ability));
        }

        public override async ETTask AfterBorn(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target,
            Entity bornEntity)
        {
            var model = target.GetComponent<ModelComponent>();
            if (model != null)
            {
                await model.WaitLoadGameObjectOver();
                if(model.IsDispose) return;
                var trans = model.GetCollectorObj<Transform>(AttachPointName);
                if (trans != null)
                {
                    var model2 = bornEntity.GetComponent<ModelComponent>();
                    if (model2 != null)
                    {
                        await model2.WaitLoadGameObjectOver();
                        if (!model.IsDispose && !model2.IsDispose)//防止创建回来父节点已经被销毁
                        {
                            model2.EntityView.SetParent(trans);
                        }
                    }
                }
                else
                {
                    Log.Error("挂点不存在"+AttachPointName);
                }
            }
        }
    }
}