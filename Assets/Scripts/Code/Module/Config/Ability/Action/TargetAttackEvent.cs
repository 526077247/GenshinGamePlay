using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)][LabelText("指定目标攻击")]
    public partial class TargetAttackEvent : ConfigAbilityAction
    {
        [NinoMember(14)]
        public TargetType TargetType = TargetType.Enemy;
        [NinoMember(10)][LabelText("*攻击者")][Tooltip("用于处理最终攻击来源以及伤害计算,仅支持指定一个,若选择结果超过1个默认取第一个")]
        public AbilityTargetting AttackTargetting = AbilityTargetting.Owner;
        [NinoMember(11)][LabelText("防御者")]
        public AbilityTargetting BeAttackTargetting = AbilityTargetting.Target;
        [NinoMember(14)][LabelText("*攻击范围检测者")][Tooltip("用于处理碰撞或触发检测,仅支持指定一个,若选择结果超过1个默认取第一个,没找到则使用攻击者进行检测")]
        public AbilityTargetting AttackCheckTargetting = AbilityTargetting.Self;
        [NinoMember(12)][ShowIf(nameof(BeAttackTargetting), AbilityTargetting.Other)]
        public ConfigSelectTargets OtherBeAttackTargets;
        [NotNull][NinoMember(13)]
        public ConfigAttackInfo AttackInfo = new ConfigAttackInfo();

        protected override void Execute(Entity actionExecuter, ActorAbility ability, ActorModifier modifier,
            Entity target)
        {
            if (TargetType == TargetType.None || AttackInfo == null) return;
            Entity attacker,attackerModel;
            using (var attackers =
                   TargetHelper.ResolveTarget(actionExecuter, ability, modifier, target, AttackTargetting))
            {
                if (attackers.Count == 0)
                {
                    Log.Error("没有找到攻击方，请检查逻辑");
                    return;
                }

                attacker = attackerModel = attackers[0];
            }

            if (AttackTargetting != AttackCheckTargetting)
            {
                using (var attackers =
                       TargetHelper.ResolveTarget(actionExecuter, ability, modifier, target, AttackCheckTargetting))
                {
                    if (attackers.Count != 0)
                    {
                        attackerModel = attackers[0];
                    }
                }
            }
            EntityManager entityManager = actionExecuter.Parent;
            bool executerIsBullet = false;
            long startTime = 0;
            var bullet = actionExecuter.GetComponent<BulletComponent>();
            if (bullet != null)
            {
                executerIsBullet = true;
                startTime = bullet.CreateTime;
            }
            long attackInfoId = IdGenerater.Instance.GenerateId();
            bool isTimeScale = false;
            using (var beAttackers = TargetHelper.ResolveTarget(actionExecuter, ability, modifier, target,
                       BeAttackTargetting, OtherBeAttackTargets))
            {
                for (int i = 0; i < beAttackers.Count; i++)
                {
                    var beAttacker = beAttackers[i];
                    if (!AttackHelper.CheckIsTarget(attacker, beAttacker, TargetType))
                        continue;
                    var len = ResolveHit(attackerModel, beAttacker, new[] {EntityType.ALL}, out var infos);
                    if (len < 1) continue;
                    var info = infos[0];
                    var hitEntity = entityManager.Get<Entity>(info.EntityId);
                    AttackResult result = AttackResult.Create(attacker.Id, hitEntity.Id, info, AttackInfo,
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
            }
        }

        private HitInfo[] defaultHit = new HitInfo[1];
        private int ResolveHit(Entity attacker, Entity beAttacker, EntityType[] filter, out HitInfo[] hitInfos)
        {
            var vcf = attacker.GetComponent<UnitModelComponent>();
            var tbc = vcf?.EntityView?.GetComponentInChildren<ColliderBoxComponent>();
            if (tbc == null && attacker is SceneEntity se)
            {
                // 没有ColliderBoxComponent的
                hitInfos = defaultHit;
                hitInfos[0] = new HitInfo()
                {
                    EntityId = beAttacker.Id,
                    HitPos = se.Position,
                    HitDir = se.Forward,
                    Distance = 0,
                    HitBoxType = HitBoxType.Normal
                };
                return 1;
            }
            var vct = beAttacker.GetComponent<UnitModelComponent>();
            var colliders = vct?.EntityView?.GetComponentsInChildren<Collider>();
            return PhysicsHelper.OverlapColliderNonAllocHitInfo(tbc, colliders, filter, CheckHitLayerType.Both,
                out hitInfos);
        }
    }
}