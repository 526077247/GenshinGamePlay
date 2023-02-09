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
            if(attacker==null||defence == null) return;
            result.DamagePercentage = result.ConfigAttackInfo.AttackProperty.DamagePercentage.Resolve(attacker, ability);
            result.DamagePercentageRatio = result.ConfigAttackInfo.AttackProperty.DamagePercentageRatio.Resolve(attacker, ability);
            result.DamageExtra = result.ConfigAttackInfo.AttackProperty.DamageExtra.Resolve(attacker, ability);
            result.BonusCritical = result.ConfigAttackInfo.AttackProperty.BonusCritical.Resolve(attacker, ability);
            result.BonusCriticalHurt = result.ConfigAttackInfo.AttackProperty.BonusCriticalHurt.Resolve(attacker, ability);
            result.ElementType = result.ConfigAttackInfo.AttackProperty.ElementType;
            result.ElementDurability = result.ConfigAttackInfo.AttackProperty.ElementDurability.Resolve(attacker, ability);
            result.ElementRankRawNum = result.ConfigAttackInfo.AttackProperty.ElementRankRawNum;
            result.EnBreakRawNum = result.ConfigAttackInfo.AttackProperty.EnBreakRawNum;
            result.EnHeadBreakRawNum = result.ConfigAttackInfo.AttackProperty.EnHeadBreakRawNum;
            result.TrueDamage = result.ConfigAttackInfo.AttackProperty.TrueDamage;
            result.IgnoreAttackerProperty = result.ConfigAttackInfo.AttackProperty.IgnoreAttackerProperty;
            result.IgnoreLevelDiff = result.ConfigAttackInfo.AttackProperty.IgnoreLevelDiff;
            result.StrikeType = result.ConfigAttackInfo.AttackProperty.StrikeType;
            result.OverrideByWeapon = result.ConfigAttackInfo.AttackProperty.OverrideByWeapon;
            result.AttackType = result.ConfigAttackInfo.AttackProperty.AttackType;
            
            //todo: CombatComponent的BeforeDamage 和AfterDamage

            if (result.BonusCritical > 0)
            {
                
            }
        }
    }
}