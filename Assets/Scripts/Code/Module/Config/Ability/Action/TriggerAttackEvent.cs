using Nino.Serialization;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class TriggerAttackEvent : ConfigAbilityAction
    {
        [NinoMember(10)]
        public TargetType TargetType;
        [NotNull] [NinoMember(11)]
        public ConfigAttackEvent AttackEvent;

        protected override void Execute(Entity applier, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            if (TargetType.None == TargetType || AttackEvent == null || AttackEvent.AttackInfo == null ||
                AttackEvent.AttackPattern == null) return;
            var len = AttackEvent.AttackPattern.ResolveHit(applier, ability, modifier, target,
                new[] {EntityType.ALL}, out var infos);
            bool isBullet = false;
            long startTime = 0;
            var bullet = applier.GetComponent<BulletComponent>();
            if (bullet != null)
            {
                isBullet = true;
                startTime = bullet.CreateTime;
            }
            for (int i = 0; i < len; i++)
            {
                if (TargetType == TargetType.None)
                    continue;
                var info = infos[i];
                var hitEntity = target.Parent.Get<Entity>(info.EntityId);
                if (TargetType == TargetType.Self && info.EntityId != target.Id)
                    continue;
                if (TargetType == TargetType.AllExceptSelf && info.EntityId == target.Id)
                    continue;
                if (TargetType == TargetType.Enemy && !AttackHelper.CheckIsEnemy(target, hitEntity))
                    continue;
                if (TargetType == TargetType.SelfCamp && !AttackHelper.CheckIsCamp(target, hitEntity))
                    continue;
                if (TargetType == TargetType.Alliance && !AttackHelper.CheckIsAlliance(target, hitEntity))
                    continue;
                AttackResult result = AttackResult.Create(target.Id, hitEntity.Id, info, AttackEvent.AttackInfo,
                    isBullet, startTime);
                AttackHelper.DamageClose(ability, modifier, result);
                result.Dispose();
            }
            
            //相机震动
            if (AttackEvent.AttackInfo.ForceCameraShake && !AttackEvent.AttackInfo.CameraShake.BroadcastOnHit &&
                AttackEvent.AttackInfo.CameraShake.ShakeType != CameraShakeType.HitVector)
            {
                Messager.Instance.Broadcast(0, MessageId.ShakeCamera, new CameraShakeParam
                    {
                        Source = (applier as Unit).Position,
                        ShakeDir = AttackEvent.AttackInfo.CameraShake.ShakeType == CameraShakeType.Center
                            ? Vector3.zero
                            : AttackEvent.AttackInfo.CameraShake.ShakeDir,
                        ShakeRange = AttackEvent.AttackInfo.CameraShake.ShakeRange,
                        ShakeFrequency = AttackEvent.AttackInfo.CameraShake.ShakeFrequency,
                        ShakeTime = AttackEvent.AttackInfo.CameraShake.ShakeTime,
                        ShakeDistance = AttackEvent.AttackInfo.CameraShake.ShakeDistance,
                        RangeAttenuation = AttackEvent.AttackInfo.CameraShake.RangeAttenuation
                    });
            }
        }
    }
}