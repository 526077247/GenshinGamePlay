using System;
using UnityEngine;

namespace TaoTie
{
    public class CombatComponent : Component, IComponent<ConfigCombat>
    {
        private FsmComponent fsm => parent.GetComponent<FsmComponent>();
        protected AttackTarget attackTarget;
        public DieStateFlag DieStateFlag;
        private ConfigCombat config;

        public bool IsInCombat { get; private set; }
        public bool CanHeHit;

        public void Init(ConfigCombat config)
        {
            this.config = config;
            if(config!=null && config.BeHit!=null)
                CanHeHit = !config.BeHit.MuteAllHit;
            attackTarget = new AttackTarget();
        }

        public void Destroy()
        {
            beforeAttack = null;
            beforeBeAttack = null;
            afterAttack = null;
            afterBeAttack = null;
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
            if (result.HitLevel >= HitLevel.Shake)//todo: 击退击飞
            {
                fsm.SetData(FSMConst.Shake, true);

                if (result.RetreatDir.sqrMagnitude > 0)
                {
                    var p = GetParent<SceneEntity>();
                    var mc = p.GetComponent<MoveComponent>();
                    mc.CharacterInput.HitImpulse += Vector3.up * result.HitImpulseY +
                                                    new Vector3(result.RetreatDir.x, 0, result.RetreatDir.z).normalized *
                                                    result.HitImpulseX;
                }
            }
            if (config != null && config.BeHit != null && !config.BeHit.MuteAllHitText 
                && (result.HitPattern == null || !result.HitPattern.MuteHitText))
            {
                Messager.Instance.Broadcast(0, MessageId.ShowDamageText, result);
            }
            //相机震动
            if (result.ConfigAttackInfo.ForceCameraShake && (result.ConfigAttackInfo.CameraShake.BroadcastOnHit ||
                result.ConfigAttackInfo.CameraShake.ShakeType == CameraShakeType.HitVector))
            {
                Messager.Instance.Broadcast(0, MessageId.ShakeCamera, new CameraShakeParam
                    {
                        Source = result.HitInfo.HitPos,
                        ShakeDir = result.HitInfo.HitDir,
                        ShakeRange = result.ConfigAttackInfo.CameraShake.ShakeRange,
                        ShakeFrequency = result.ConfigAttackInfo.CameraShake.ShakeFrequency,
                        ShakeTime = result.ConfigAttackInfo.CameraShake.ShakeTime,
                        ShakeDistance = result.ConfigAttackInfo.CameraShake.ShakeDistance,
                        RangeAttenuation = result.ConfigAttackInfo.CameraShake.RangeAttenuation
                    });
            }
        }

        /// <summary>
        /// 立刻使用技能
        /// </summary>
        /// <param name="skillId"></param>
        public void UseSkillImmediately(int skillId)
        {
            fsm.SetData(FSMConst.UseSkill, true);
            fsm.SetData(FSMConst.SkillId, skillId);
        }
        /// <summary>
        /// 立即停止使用技能
        /// </summary>
        public void ReleaseSkillImmediately()
        {
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
            CanHeHit = false;
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
            if(config.CombatLock == null) return;
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
        }
    }
}