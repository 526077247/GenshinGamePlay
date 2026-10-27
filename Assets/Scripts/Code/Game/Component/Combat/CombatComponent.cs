using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class CombatComponent : Component, IComponent<ConfigCombat>
    {
        private FsmComponent fsm => parent.GetComponent<FsmComponent>();
        protected AttackTarget attackTarget = new AttackTarget();
        public DieStateFlag DieStateFlag;
        private ConfigCombat config;

        private HashSet<long> hitAttackRecords = new HashSet<long>();

        /// <summary>「受击方没有移动组件」的告警每个实体只喊一次，否则每次挨打都刷。</summary>
        private bool hitImpulseNoMoveLogged;

        /// <summary>
        /// 非循环命中(Recurring=false)模式下,判断该攻击实例是否已经命中过当前实体
        /// </summary>
        public bool HasAlreadyHit(long attackInstanceId)
        {
            return hitAttackRecords.Contains(attackInstanceId);
        }

        public void RecordHit(long attackInstanceId)
        {
            hitAttackRecords.Add(attackInstanceId);
        }

        public void RemoveHitRecord(long attackInstanceId)
        {
            hitAttackRecords.Remove(attackInstanceId);
        }

        public bool IsInCombat { get; private set; }
        public bool CanBeHit;

        public void Init(ConfigCombat config)
        {
            IsInCombat = false;
            this.config = config;
            if(config!=null && config.BeHit!=null)
                CanBeHit = !config.BeHit.MuteAllHit;
            attackTarget.Reset();
            Messager.Instance.AddListener<long>(0, MessageId.ClearHitRecord, RemoveHitRecord);
        }

        public void Destroy()
        {
            Messager.Instance.RemoveListener<long>(0, MessageId.ClearHitRecord, RemoveHitRecord);
            beforeAttack = null;
            beforeBeAttack = null;
            afterAttack = null;
            afterBeAttack = null;
            IsInCombat = false;
            config = null;
            CanBeHit = false;
            hitAttackRecords.Clear();
        }

        /// <summary>
        /// 造成伤害前
        /// </summary>
        public event Action<AttackResult, CombatComponent> beforeAttack;

        /// <summary>
        /// 受到伤害前
        /// </summary>
        public event Action<AttackResult, CombatComponent> beforeBeAttack;

        /// <summary>
        /// 造成伤害后
        /// </summary>
        public event Action<AttackResult, CombatComponent> afterAttack;

        /// <summary>
        /// 受到伤害后
        /// </summary>
        public event Action<AttackResult, CombatComponent> afterBeAttack;

        /// <summary>
        /// 造成伤害前
        /// </summary>
        public void BeforeAttack(AttackResult result, CombatComponent other)
        {
            beforeAttack?.Invoke(result, other);
        }

        /// <summary>
        /// 受到伤害前
        /// </summary>
        public void BeforeBeAttack(AttackResult result, CombatComponent other)
        {
            beforeBeAttack?.Invoke(result, other);
        }

        /// <summary>
        /// 造成伤害后
        /// </summary>
        public void AfterAttack(AttackResult result, CombatComponent other)
        {
            afterAttack?.Invoke(result, other);
        }

        /// <summary>
        /// 受到伤害后
        /// </summary>
        public void AfterBeAttack(AttackResult result, CombatComponent other)
        {
            afterBeAttack?.Invoke(result, other);
            if (result.HitLevel >= HitLevel.Shake)
            {
                var fsmComponent = fsm;
                // Shake 属于「可选表现参数」：多数单位的 FsmController 根本没声明它，
                // 直接 SetData 会让 FsmComponent 每次命中刷一条 "SetData Can Not Find bool Param: Shake"
                // （表现为「打得越多日志越脏」，且没人能从中看出是哪份配置缺了参数）。
                // 没声明就只是不做受击抖动，不算配置错误。
                if (fsmComponent != null && fsmComponent.KeyExist(FSMConst.Shake))
                    fsmComponent.SetData(FSMConst.Shake, true);

                DispatchHitImpulse(result, other);
            }
            if (config != null && config.BeHit != null && !config.BeHit.MuteAllHitText 
                && (result.HitPattern == null || !result.HitPattern.MuteHitText))
            {
                Messager.Instance.Broadcast(0, MessageId.ShowDamageText, result);
            }
            //击中相机震动
            if (result.ConfigAttackInfo != null && result.ConfigAttackInfo.ForceCameraShake
                && result.ConfigAttackInfo.CameraShake != null
                && (result.ConfigAttackInfo.CameraShake.BroadcastOnHit
                ||result.ConfigAttackInfo.CameraShake.ShakeType == CameraShakeType.HitVector))
            {
                var para = new CameraShakeParam
                {
                    Id  = result.Id,
                    Source = result.HitInfo.HitPos,
                    ShakeRange = result.ConfigAttackInfo.CameraShake.ShakeRange,
                    ShakeFrequency = result.ConfigAttackInfo.CameraShake.ShakeFrequency,
                    ShakeTime = result.ConfigAttackInfo.CameraShake.ShakeTime,
                    ShakeDistance = result.ConfigAttackInfo.CameraShake.ShakeDistance,
                    RangeAttenuation = result.ConfigAttackInfo.CameraShake.RangeAttenuation
                };
                switch (result.ConfigAttackInfo.CameraShake.ShakeType)
                {
                    case CameraShakeType.HitVector:
                        para.ShakeDir = result.HitInfo.HitDir;
                        break;
                    case CameraShakeType.CustomVector:
                        para.ShakeDir = result.ConfigAttackInfo.CameraShake.ShakeDir;
                        break;
                    case CameraShakeType.Center:
                        para.ShakeDir = CameraManager.Instance.MainCamera().transform.forward;
                        break;
                }
                Messager.Instance.Broadcast(0, MessageId.ShakeCamera, para);
            }
        }

        /// <summary>
        /// 把 <see cref="AttackResult"/> 的击退数据交给移动组件的统一入口。
        /// 抗性缩放与「是否算击飞」由 <see cref="MoveComponent.ApplyHitImpulse"/> 判定（受击方口径），
        /// 这里只负责把「方向 × 力度 + 抬升」拼成世界系冲量。
        ///
        /// 为什么不再直写 <see cref="MoveInput.HitImpulse"/>：
        /// 那样 SimpleMove 单位（没有击退通道）会被静默丢弃，抗性表也永远查不到受击方配置。
        /// </summary>
        private void DispatchHitImpulse(AttackResult result, CombatComponent attacker)
        {
            if (result.HitImpulseX == 0f && result.HitImpulseY == 0f) return;

            var self = GetParent<SceneEntity>();
            var mc = self != null ? self.GetComponent<MoveComponent>() : null;
            if (mc == null)
            {
                // 固定靶（MuZhuang 等没有 Move 配置段的单位）根本没有 MoveComponent，
                // 冲量无处落地。这条以前完全静默，是「配了击退却没反应」最容易误判的一种，
                // 所以按实体喊一次，并直接点名是「目标没配 Move」而不是「击退坏了」。
                if (!hitImpulseNoMoveLogged)
                {
                    hitImpulseNoMoveLogged = true;
                    Log.Info($"实体 {self?.Id} 没有 MoveComponent（ConfigActor 未配置 Move 段），无处承受击退，冲量被丢弃");
                }
                return;
            }

            Vector3 dir = result.RetreatDir;
            dir.y = 0f;
            if (dir.sqrMagnitude < 1e-6f)
            {
                // TargetHelper.ResolveTarget 拿不到双方位置时会返回零向量。这种时候如果照直把冲量丢掉，
                // 配了击退的攻击就成了「打不退人」，且没有任何报错线索。
                // 退化方向取「攻击起点 → 受击者」：起点走 ResolveAttackOrigin，
                // 所以子弹（attacker 解析成子弹自己）也能拿到射手而不是贴在脸上的子弹。
                var attackerEntity = attacker != null ? attacker.GetParent<SceneEntity>() : null;
                attackerEntity = TargetHelper.ResolveAttackOrigin(attackerEntity, self, result) as SceneEntity;
                if (attackerEntity != null)
                {
                    dir = self.Position - attackerEntity.Position;
                    dir.y = 0f;
                }
            }

            Vector3 horizontal = dir.sqrMagnitude > 1e-6f ? dir.normalized * result.HitImpulseX : Vector3.zero;
            mc.ApplyHitImpulse(new Vector3(horizontal.x, result.HitImpulseY, horizontal.z), result.HitImpulseType);
        }

        /// <summary>
        /// 立刻使用技能
        /// </summary>
        /// <param name="skillId"></param>
        public void UseSkillImmediately(int skillId)
        {
            if (fsm == null) return;
            fsm.SetData(FSMConst.UseSkill, true);
            fsm.SetData(FSMConst.SkillId, skillId);
        }
        /// <summary>
        /// 立即停止使用技能
        /// </summary>
        public void ReleaseSkillImmediately()
        {
            if (fsm == null) return;
            fsm.SetData(FSMConst.UseSkill, false);
            fsm.SetData(FSMConst.SkillId, 0);
        }

        /// <summary>
        /// 被击杀
        /// </summary>
        /// <param name="killerID"></param>
        /// <param name="dieType"></param>
        public void DoKill(long killerID, DieStateFlag dieType)
        {
            DieStateFlag = dieType;
            Messager.Instance.Broadcast(Id, MessageId.OnBeKill, config?.Die, dieType);
            Messager.Instance.Broadcast(0, MessageId.OnBeKill, GetParent<Actor>());
            OnBeKill();
            if(killerID != Id)
                Messager.Instance.Broadcast(killerID, MessageId.OnKill, Id);
        }

        public void OnBeKill()
        {
            CanBeHit = false;
            var configDie = config?.Die;
            if (configDie != null)
            {
                var unit = GetParent<Unit>();
                if (unit == null) return;
                bool delayRecycle = configDie.DieEndTime != 0;
                
                if (delayRecycle)
                {
                    if (configDie.DieEndTime > 0)
                    {
                        parent.DelayDispose(configDie.DieEndTime);
                    }
                    else
                    {
                        parent.Dispose();
                    }
                    Dispose();
                }
                else
                {
                    parent.Dispose();
                }
            }
            else
            {
                parent.Dispose();
            }
        }

        public Entity GetAttackTarget()
        {
            if (attackTarget.RuntimeID != 0)
            {
                return parent.Parent.Get(attackTarget.RuntimeID);
            }

            return null;
        }
        public void SetAttackTarget(long attackTargetRuntimeID, string lockPoint)
        {
            attackTarget.RuntimeID = attackTargetRuntimeID;
            attackTarget.LockedPoint = lockPoint;
        }
        public void SelectAttackTarget(bool force)
        {
            if(config == null || config.CombatLock == null) return;
            if (attackTarget.RuntimeID == 0 || force)
            {
                var unit = GetParent<Unit>();
                var count = PhysicsHelper.OverlapSphereNonAllocHitInfo(unit.Position, config.CombatLock.OverrideRange,
                    new[] {EntityType.Monster, EntityType.Avatar}, CheckHitLayerType.OnlyHitBox,
                    out var hitInfos);
                float angle = 180;
                attackTarget.RuntimeID = 0;
                for (int i = 0; i < count; i++)
                {
                    var hitEntity = unit.Parent.Get(hitInfos[i].EntityId);
                    if (hitEntity is Unit other && AttackHelper.CheckIsEnemy(unit, hitEntity))
                    {
                        var dir = other.Position - unit.Position;
                        var a = Mathf.Abs(Vector3.Angle(dir, unit.Forward));
                        if (a < angle && a< config.CombatLock.AimAngle)
                        {
                            attackTarget.LockedPoint = null;
                            attackTarget.RuntimeID = hitInfos[i].EntityId;
                            angle = a;
                        }
                    }
                }
            }
        }

        public void SetCombatState(bool inCombat)
        {
            if (inCombat != IsInCombat)
            {
                IsInCombat = inCombat;
                Messager.Instance.Broadcast(Id, MessageId.CombatStateChange, IsInCombat);
            }
            if (fsm != null)
                fsm.SetData(FSMConst.InCombat, inCombat);
        }
    }
}