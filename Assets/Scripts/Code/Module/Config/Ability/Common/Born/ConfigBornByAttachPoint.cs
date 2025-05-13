using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigBornByAttachPoint : ConfigBornType
    {
        [LabelText("挂点")]
        [NinoMember(5)] public string AttachPointName;
        [LabelText("挂点所属目标")]
        [NinoMember(6)] public AttachPointTargetType AttachPointTargetType;
        [LabelText("是否挂载到挂点")]
        [NinoMember(7)] public bool AttachToPoint = true;
        [NinoMember(8)][LabelText("*重写方向")][Tooltip("是否需要重新指定创建时的方向，不勾选表示使用挂点方向")]
        public bool OverrideForward = false;
        [LabelText("*是否指向目标")][ShowIf(nameof(OverrideForward))][Tooltip("勾选表示使用出生点指向目标点的方向，不勾选表示使用目标当前方向作为出生方向(目标不存在时使用挂点方向)")]
        [NinoMember(9)] public bool LookAtTarget = true;
        [LabelText("目标")][ShowIf(nameof(OverrideForward))]
        [NinoMember(10)] public AbilityTargetting ForwardUseType = AbilityTargetting.Target;
        public override Vector3 ResolvePos(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            var entity = TargetHelper.ResolveTarget(actor, ability, modifier, target, AttachPointTargetType);
            var model = entity?.GetComponent<UnitModelComponent>();
            if (model != null)
            {
                var trans = model.GetCollectorObj<Transform>(AttachPointName);
                if (trans != null)
                {
                    return trans.position + trans.rotation * PositionOffset.Resolve(entity, ability);
                }
                else
                {
                    Log.Error("挂点不存在"+AttachPointName);
                }
            }

            return PositionOffset.Resolve(target, ability);
        }

        public override Quaternion ResolveRot(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            if (!OverrideForward)
            {
                var entity = TargetHelper.ResolveTarget(actor, ability, modifier, target, AttachPointTargetType);
                var model = entity?.GetComponent<UnitModelComponent>();
                if (model != null)
                {
                    var trans = model.GetCollectorObj<Transform>(AttachPointName);
                    if (trans != null)
                    {
                        return Quaternion.Euler(trans.eulerAngles + RotationOffset.Resolve(entity, ability));
                    }
                    else
                    {
                        Log.Error("挂点不存在"+AttachPointName);
                    }
                }
                return Quaternion.Euler(RotationOffset.Resolve(target, ability));
            }
            else
            {
                var entities = TargetHelper.ResolveTarget(actor, ability, modifier, target, ForwardUseType);
                if (entities.Count > 0)
                {
                    for (int i = 0; i < entities.Count; i++)
                    {
                        var entity = entities[i];
                        if (entity is SceneEntity se)
                        {
                            if (LookAtTarget)
                            {
                                var dir = se.Position - ResolvePos(actor, ability, modifier, target);
                                var rot = Quaternion.LookRotation(dir, Vector3.up);
                                if (RotationOffset is ZeroVector3)
                                {
                                    return rot;
                                }
                                return Quaternion.Euler(rot.eulerAngles + RotationOffset.Resolve(entity, ability));
                            }
                            else
                            {
                                return Quaternion.Euler(se.Rotation.eulerAngles + RotationOffset.Resolve(entity, ability));
                            }
                        }
                    }
                }
                entities.Dispose();
                return Quaternion.Euler(RotationOffset.Resolve(target, ability));
            }
        }

        public override async ETTask AfterBorn(Entity actor, ActorAbility ability, ActorModifier modifier, Entity target,
            Entity bornEntity)
        {
            if(!AttachToPoint) return;
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