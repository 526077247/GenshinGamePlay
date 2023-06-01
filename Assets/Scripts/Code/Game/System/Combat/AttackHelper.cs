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
            if (actor.Type == EntityType.Avatar && other.Type == EntityType.Monster)
                return true;
            if (actor.Type == EntityType.Monster && other.Type == EntityType.Avatar)
                return true;
            
            //todo:
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
            //todo:
            return actor.Type == other.Type;
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
                foreach (var item in result.ConfigAttackInfo.AttackProperty.EnBreak)
                {
                    result.EnBreak.Add(item.Key,item.Value.Resolve(attacker, ability));
                }
            }
            result.TrueDamage = result.ConfigAttackInfo.AttackProperty.TrueDamage;
            result.IgnoreLevelDiff = result.ConfigAttackInfo.AttackProperty.IgnoreLevelDiff;
            result.StrikeType = result.ConfigAttackInfo.AttackProperty.StrikeType;
            result.AttackType = result.ConfigAttackInfo.AttackProperty.AttackType;


            combatA.BeforeAttack(result, combatD);
            if (!result.IsEffective) return; //被取消
            combatD.BeforeBeAttack(result, combatA);
            if (!result.IsEffective) return; //被取消

            result.FinalRealDamage = 0;

            //等级差异加伤减伤
            if (!result.IgnoreLevelDiff)
            {
                result.DamagePercentageRatio *= (float)Math.Tanh(numA.GetAsFloat(NumericType.Lv)/10) + 1;
            }
            
            //非真伤，防御减免
            if (!result.TrueDamage)
            {
                var lv = Mathf.Max(1, numD.GetAsFloat(NumericType.Lv));
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
            result.FinalRealDamage = (int) (
                result.DamagePercentage * result.DamagePercentageRatio *
                (result.IsCritical ? (result.BonusCriticalHurt + 1) : 1) + result.DamageExtra);
            Log.Info("最终伤害： "+result.FinalRealDamage +" HitBoxType: "+ result.HitInfo.HitBoxType);
            //修改血量
            var finalHp = numD.GetAsInt(NumericType.HpBase) - result.FinalRealDamage;
            if (finalHp < 0) finalHp = 0;
            numD.Set(NumericType.HpBase, finalHp);
            //破霸体
            if (result.EnBreak.TryGetValue(result.HitInfo.HitBoxType,out float value))
            {
                numD.Set(NumericType.ENBase, numD.GetAsInt(NumericType.ENBase) - value);
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