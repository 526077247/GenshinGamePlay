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
            var entity = TargetHelper.ResolveTarget(actor, ability, modifier, target, AttachPointTargetType);
            var model = entity?.GetComponent<UnitModelComponent>();
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
            var entity = TargetHelper.ResolveTarget(actor, ability, modifier, target, AttachPointTargetType);
            var model = entity?.GetComponent<UnitModelComponent>();
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
            var attachPoint = await GetAttachPoint(actor,ability,modifier,target);
            if (attachPoint != null)
            {
                var model = bornEntity.GetComponent<UnitModelComponent>();
                if (model != null)
                {
                    await model.SetAttachPoint(attachPoint);
                }
                var effect = bornEntity.GetComponent<EffectModelComponent>();
                if (effect != null)
                {
                    await effect.SetAttachPoint(attachPoint);
                }
            }
            else
            {
                Log.Error("挂点不存在" + AttachPointName);
            }
        }

        private async ETTask<Transform> GetAttachPoint(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            var entity = TargetHelper.ResolveTarget(actor, ability, modifier, target, AttachPointTargetType);
            var model = entity?.GetComponent<UnitModelComponent>();
            if (model != null)
            {
                await model.WaitLoadGameObjectOver();
                if(model.IsDispose) return null;
                return model.GetCollectorObj<Transform>(AttachPointName);
            }
            return null;
        }
        
    }
}