using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)][LabelText("*范围检测攻击")][Tooltip("攻击者为Target")]
    public partial class TriggerAttackEvent : ConfigAbilityAction
    {
        [NinoMember(10)]
        public TargetType TargetType = TargetType.Enemy;
        [NotNull] [NinoMember(11)]
        public ConfigAttackEvent AttackEvent = new ConfigAttackEvent();
        [NinoMember(12)][Range(0,1)][LabelText("夹角权值（负相关）")][BoxGroup("碰撞盒优先级")]
        public float A;
        [NinoMember(13)][Range(0,1)][LabelText("高度差权值（正相关）")][BoxGroup("碰撞盒优先级")]
        public float B;
        protected override void Execute(Entity actionExecuter, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            if (TargetType.None == TargetType || AttackEvent == null || AttackEvent.AttackInfo == null ||
                AttackEvent.AttackPattern == null) return;
            var len = AttackEvent.AttackPattern.ResolveHit(actionExecuter, ability, modifier, target,
                new[] {EntityType.ALL}, out var infos);
            EntityManager entityManager = actionExecuter.Parent;
            bool executerIsBullet = false;
            long startTime = 0;
            var bullet = actionExecuter.GetComponent<BulletComponent>();
            if (bullet != null)
            {
                executerIsBullet = true;
                startTime = bullet.CreateTime;
            }

            UnOrderMultiMap<long, HitInfo> temp = ObjectPool.Instance.Fetch<UnOrderMultiMap<long, HitInfo>>();
            for (int i = 0; i < len; i++)
            {
                var info = infos[i];
                var hitEntity = entityManager.Get<Entity>(info.EntityId);
                if (!AttackHelper.CheckIsTarget(target, hitEntity, TargetType))
                    continue;
                temp.Add(info.EntityId, info);
            }

            long attackInfoId = IdGenerater.Instance.GenerateId();
            bool isTimeScale = false;
            foreach (var item in temp)
            {
                var info = item.Value[0];
                if (item.Value.Count > 1)
                {
                    // 根据最佳点公式计算每个受击点最小权值
                    var minWeight = AttackHelper.CalcWeightBaseAngle(actionExecuter, info, A, B);
                    for (int i = 1; i < item.Value.Count; i++)
                    {
                        var weight = AttackHelper.CalcWeightBaseAngle(actionExecuter, item.Value[i], A, B);
                        if (weight < minWeight)
                        {
                            minWeight = weight;
                            info = item.Value[i];
                        }
                    }
                }

                var hitEntity = entityManager.Get<Entity>(info.EntityId);
                AttackResult result = AttackResult.Create(target.Id, hitEntity.Id, info, AttackEvent.AttackInfo,
                    executerIsBullet, startTime,attackInfoId);
                AttackHelper.DamageClose(ability, modifier, result);
                //时停
                if (!isTimeScale && result.HitPattern != null)
                {
                    if (result.HitPattern.HitHaltTime > 0)
                    {
                        //todo:格挡是否时停判断,临时用最终伤害大于0判定
                        if (result.HitPattern.CanBeDefenceHalt || result.FinalRealDamage > 0)
                        {
                            GameTimerManager.Instance.SetTimeScale(result.HitPattern.HitHaltTimeScale,
                                result.HitPattern.HitHaltTime);
                            isTimeScale = true;
                        }
                    }
                }

                result.Dispose();
            }

            temp.Clear();
            //非击中广播的相机震动
            if (AttackEvent.AttackInfo.ForceCameraShake && !AttackEvent.AttackInfo.CameraShake.BroadcastOnHit
                && AttackEvent.AttackInfo.CameraShake.ShakeType != CameraShakeType.HitVector)
            {
                Messager.Instance.Broadcast(0, MessageId.ShakeCamera, new CameraShakeParam
                {
                    Id = attackInfoId,
                    Source = (target as SceneEntity).Position,
                    ShakeDir = AttackEvent.AttackInfo.CameraShake.ShakeType == CameraShakeType.Center
                        ? CameraManager.Instance.MainCamera().transform.forward
                        : AttackEvent.AttackInfo.CameraShake.ShakeDir,
                    ShakeRange = AttackEvent.AttackInfo.CameraShake.ShakeRange,
                    ShakeFrequency = AttackEvent.AttackInfo.CameraShake.ShakeFrequency,
                    ShakeTime = AttackEvent.AttackInfo.CameraShake.ShakeTime,
                    ShakeDistance = AttackEvent.AttackInfo.CameraShake.ShakeDistance,
                    RangeAttenuation = AttackEvent.AttackInfo.CameraShake.RangeAttenuation
                });
            }
            ObjectPool.Instance.Recycle(temp);
        }
    }
}