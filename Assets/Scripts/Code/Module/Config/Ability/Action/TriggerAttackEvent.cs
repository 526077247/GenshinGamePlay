using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class TriggerAttackEvent : ConfigAbilityAction
    {
        private static readonly UnOrderMultiMap<long, HitInfo> temp = new UnOrderMultiMap<long, HitInfo>();
        [NinoMember(10)]
        public TargetType TargetType;
        [NotNull] [NinoMember(11)]
        public ConfigAttackEvent AttackEvent;
        [NinoMember(12)][Range(0,1)][LabelText("夹角权值（负相关）")][BoxGroup("碰撞盒优先级")]
        public float A;
        [NinoMember(13)][Range(0,1)][LabelText("高度差权值（正相关）")][BoxGroup("碰撞盒优先级")]
        public float B;
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
                temp.Add(info.EntityId,info);
            }

            foreach (var item in temp)
            {
                var info = item.Value[0];
                if (item.Value.Count > 1)
                {
                    // 根据最佳点公式计算每个受击点最小权值
                    var minWeight = AttackHelper.CalcWeightBaseAngle(applier,info,A,B);
                    for (int i = 1; i < item.Value.Count; i++)
                    {
                        var weight = AttackHelper.CalcWeightBaseAngle(applier,item.Value[i],A,B);
                        if (weight < minWeight)
                        {
                            minWeight = weight;
                            info = item.Value[i];
                        }
                    }
                }
                var hitEntity = target.Parent.Get<Entity>(info.EntityId);
                AttackResult result = AttackResult.Create(target.Id, hitEntity.Id, info, AttackEvent.AttackInfo,
                    isBullet, startTime);
                AttackHelper.DamageClose(ability, modifier, result);
                result.Dispose();
            }
            temp.Clear();
            
            //相机震动
            if (AttackEvent.AttackInfo.ForceCameraShake && !AttackEvent.AttackInfo.CameraShake.BroadcastOnHit &&
                AttackEvent.AttackInfo.CameraShake.ShakeType != CameraShakeType.HitVector && applier is Unit u)
            {
                Messager.Instance.Broadcast(0, MessageId.ShakeCamera, new CameraShakeParam
                    {
                        Source = u.Position,
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