using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TaoTie
{
    public static class AttackHelper
    {
        /// <summary>
        /// 检查是否是敌人
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool CheckIsEnemy(Entity actor, Entity other)
        {
            if (actor is Actor a && other is Actor b)
            {
                return a.CampId != b.CampId;
            }
            if (actor.Type == EntityType.Avatar && other.Type == EntityType.Monster)
                return true;
            if (actor.Type == EntityType.Monster && other.Type == EntityType.Avatar)
                return true;
            return false;
        }
        /// <summary>
        /// 检查是否是队友
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool CheckIsCamp(Entity actor, Entity other)
        {
            if (actor is Actor a && other is Actor b)
            {
                return a.CampId == b.CampId;
            }
            return false;
        }
        /// <summary>
        /// 检查是否同阵营
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool CheckIsAlliance(Entity actor, Entity other)
        {
            //todo:
            return actor.Type == other.Type;
        }

        /// <summary>
        /// 检查是否是敌人
        /// </summary>
        /// <param name="campId1"></param>
        /// <param name="campId2"></param>
        /// <returns></returns>
        public static bool CheckIsEnemyCamp(uint campId1,uint campId2)
        {
            //todo:
            return campId1 != campId2;
        }

        /// <summary>
        /// 计算碰撞权值
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="info"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float CalcWeightBaseAngle(Entity attacker, HitInfo info,float a,float b)
        {
            var u = attacker as Actor;
            if (b >= 1)
            {
                b = 1 - float.Epsilon;
            }
            float temp1 = (1 - Mathf.Cos(Mathf.Deg2Rad * Vector3.Angle(info.HitDir, info.HitPos - u.Position))) / 2;
            temp1 = temp1 * (1 - a) + a;
            float temp2 = Mathf.Abs(b / (1 - b) * (info.HitPos.y - u.Position.y + u.ConfigActor.Common.Height / 2)) + 1;
            return Mathf.Abs(info.Distance * temp1 * temp2);
        }
        
        /// <summary>
        /// 结算伤害流程
        /// </summary>
        /// <param name="ability"></param>
        /// <param name="modifier"></param>
        /// <param name="result"></param>
        public static void DamageClose(ActorAbility ability, ActorModifier modifier, AttackResult result)
        {
            var em = ability.Parent.GetParent<Entity>().Parent;
            var attacker = em.Get<Entity>(result.AttackerId);
            var defence = em.Get<Entity>(result.DefenseId);
            if (attacker == null || defence == null) return;
            var combatA = attacker.GetComponent<CombatComponent>();
            var combatD = defence.GetComponent<CombatComponent>();
            if (combatA == null || combatD == null) return;
            if (!combatD.CanHeHit) return;
            var numA = attacker.GetComponent<NumericComponent>();
            var numD = defence.GetComponent<NumericComponent>();
            if (numA == null || numD == null) return;
            result.DamagePercentage =
                result.ConfigAttackInfo.AttackProperty.DamagePercentage.Resolve(attacker, ability);
            result.DamagePercentageRatio =
                result.ConfigAttackInfo.AttackProperty.DamagePercentageRatio.Resolve(attacker, ability);
            result.DamageExtra = result.ConfigAttackInfo.AttackProperty.DamageExtra.Resolve(attacker, ability);
            result.BonusCritical = result.ConfigAttackInfo.AttackProperty.BonusCritical.Resolve(attacker, ability);
            result.BonusCriticalHurt =
                result.ConfigAttackInfo.AttackProperty.BonusCriticalHurt.Resolve(attacker, ability);
            if (result.ConfigAttackInfo.AttackProperty.EnBreak != null)
            {
                if (result.ConfigAttackInfo.AttackProperty.EnBreak.TryGetValue(result.HitInfo.HitBoxType, out var data))
                {
                    result.EnBreak = (int)data.Resolve(attacker, ability);
                }
            }
            result.TrueDamage = result.ConfigAttackInfo.AttackProperty.TrueDamage;
            result.IgnoreLevelDiff = result.ConfigAttackInfo.AttackProperty.IgnoreLevelDiff;
            result.StrikeType = result.ConfigAttackInfo.AttackProperty.StrikeType;
            result.AttackType = result.ConfigAttackInfo.AttackProperty.AttackType;

            result.HitLevel = result.HitPattern.HitLevel;
            result.HitImpulseX = result.HitPattern.HitImpulseX.Resolve(attacker, ability);
            result.HitImpulseY = result.HitPattern.HitImpulseY.Resolve(attacker, ability);
            //todo: 冲刺状态、击退方向计算

            combatA.BeforeAttack(result, combatD);
            if (!result.IsEffective) return; //被取消
            combatD.BeforeBeAttack(result, combatA);
            if (!result.IsEffective) return; //被取消

            result.FinalRealDamage = 0;

            //子弹，根据距离衰减伤害
            if (result.IsBullet && result.ConfigAttackInfo.BulletWane != null)
            {
                //todo
            }

            //等级差异加伤减伤
            if (!result.IgnoreLevelDiff)
            {
                var lvDiff = numA.GetAsFloat(NumericType.Lv) - numD.GetAsFloat(NumericType.Lv);
                result.DamagePercentageRatio *= (float)Math.Tanh(lvDiff/10) + 1;
            }
            
            //非真伤，防御减免
            if (!result.TrueDamage)
            {
                //攻击方的
                var lv = Mathf.Max(1, numA.GetAsFloat(NumericType.Lv));
                var flag = 100 * lv;
                var def = Mathf.Max(numD.GetAsFloat(NumericType.DEF), 1);
                result.DamagePercentageRatio *= flag / (def + flag);
            }
            
            //暴击
            result.IsCritical = false;
            if (result.BonusCritical > 0)
            {
                var percent = Random.Range(0, 100);
                if (percent < result.BonusCritical * 100)
                {
                    result.IsCritical = true;
                }
            }
            
            //最终伤害
            result.FinalRealDamage = (int) (Mathf.Max(0, result.DamagePercentage * result.DamagePercentageRatio) *
                (result.IsCritical ? (result.BonusCriticalHurt + 1) : 1) + result.DamageExtra);
            Log.Info($"最终伤害： {result.FinalRealDamage} HitBoxType: {result.HitInfo.HitBoxType} \r\nFrom:{attacker.Id} To:{defence.Id}");
            //修改血量
            var finalHp = numD.GetAsInt(NumericType.HpBase) - result.FinalRealDamage;
            if (finalHp < 0) finalHp = 0;
            numD.Set(NumericType.HpBase, finalHp);
            //破霸体
            if (result.EnBreak > 0)
            {
                numD.Set(NumericType.ENBase, numD.GetAsInt(NumericType.ENBase) - result.EnBreak);
            }

            combatD.AfterBeAttack(result, combatA);
            combatA.AfterAttack(result, combatD);

            if (finalHp == 0)
            {
                combatD.DoKill(combatA.Id, DieStateFlag.None); //todo: State
            }
        }
    }
}