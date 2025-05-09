﻿using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)][LabelText("指定目标攻击")]
    public partial class TargetAttackEvent : ConfigAbilityAction
    {
        [NinoMember(14)]
        public TargetType TargetType = TargetType.Enemy;
        [NinoMember(10)][LabelText("*攻击者")][Tooltip("仅支持指定一个,若选择结果超过1个默认取第一个")]
        public AbilityTargetting AttackTargetting = AbilityTargetting.Owner;
        [NinoMember(11)][LabelText("防御者")]
        public AbilityTargetting BeAttackTargetting = AbilityTargetting.Target;
        [NinoMember(12)][ShowIf(nameof(BeAttackTargetting), AbilityTargetting.Other)]
        public ConfigSelectTargets OtherBeAttackTargets;
        [NotNull][NinoMember(13)]
        public ConfigAttackInfo AttackInfo = new ConfigAttackInfo();

        protected override void Execute(Entity actionExecuter, ActorAbility ability, ActorModifier modifier,
            Entity target)
        {
            if (TargetType == TargetType.None || AttackInfo == null) return;
            Entity attacker;
            using (var attackers =
                   TargetHelper.ResolveTarget(actionExecuter, ability, modifier, target, AttackTargetting))
            {
                if (attackers.Count == 0)
                {
                    Log.Error("没有找到攻击方，请检查逻辑");
                    return;
                }

                attacker = attackers[0];
            }

            EntityManager entityManager = actionExecuter.Parent;
            bool isBullet = false;
            long startTime = 0;
            var bullet = actionExecuter.GetComponent<BulletComponent>();
            if (bullet != null)
            {
                isBullet = true;
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
                    var len = ResolveHit(attacker, beAttacker, new[] {EntityType.ALL}, out var infos);
                    if (len < 1) continue;
                    var info = infos[0];
                    var hitEntity = entityManager.Get<Entity>(info.EntityId);
                    AttackResult result = AttackResult.Create(attacker.Id, hitEntity.Id, info, AttackInfo,
                        isBullet, startTime,attackInfoId);
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

        private int ResolveHit(Entity attacker, Entity beAttacker, EntityType[] filter, out HitInfo[] hitInfos)
        {
            var vcf = attacker.GetComponent<UnitModelComponent>();
            var tbc = vcf?.EntityView?.GetComponentInChildren<ColliderBoxComponent>();
            var vct = beAttacker.GetComponent<UnitModelComponent>();
            var colliders = vct?.EntityView?.GetComponentsInChildren<Collider>();
            return PhysicsHelper.OverlapColliderNonAllocHitInfo(tbc, colliders, filter, CheckHitLayerType.Both,
                out hitInfos);
        }
    }
}