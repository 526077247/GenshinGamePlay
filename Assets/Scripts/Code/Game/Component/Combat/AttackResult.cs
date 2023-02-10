using System;
using System.Collections.Generic;

namespace TaoTie
{
    public class AttackResult: IDisposable
    {
        public long AttackerId;
        public long DefenseId;
        public HitInfo HitInfo;
        public ConfigAttackInfo ConfigAttackInfo;
        
        /// <summary>
        /// 伤害值
        /// </summary>
        public float DamagePercentage;
        /// <summary>
        /// 伤害比例
        /// </summary>
        public float DamagePercentageRatio;
        /// <summary>
        /// 击打类型
        /// </summary>
        public StrikeType StrikeType;
        /// <summary>
        /// 破霸体值
        /// </summary>
        public readonly Dictionary<HitBoxType, float> EnBreak = new Dictionary<HitBoxType, float>();
        /// <summary>
        /// 攻击类型
        /// </summary>
        public AttackType AttackType;
        /// <summary>
        /// 额外伤害值(暂定为不受任何因素影响的固定伤害)
        /// </summary>
        public float DamageExtra;
        /// <summary>
        /// 暴击率
        /// </summary>
        public float BonusCritical;
        /// <summary>
        /// 暴击伤害
        /// </summary>
        public float BonusCriticalHurt;
        /// <summary>
        /// 忽略等级差距带来的衰减
        /// </summary>
        public bool IgnoreLevelDiff;
        /// <summary>
        /// 是否真伤
        /// </summary>
        public bool TrueDamage;

        /// <summary>
        /// 是否有效
        /// </summary>
        public bool IsEffective;

        /// <summary>
        /// 是否暴击
        /// </summary>
        public bool IsCritical;
        /// <summary>
        /// 最后真正造成的伤害(造成伤害后才会赋值)
        /// </summary>
        public int FinalRealDamage;
        public static AttackResult Create(long attackerId,long defenseId, HitInfo info,ConfigAttackInfo config)
        {
            AttackResult res = new AttackResult();
            res.AttackerId = attackerId;
            res.DefenseId = defenseId;
            res.HitInfo = info;
            res.ConfigAttackInfo = config;
            res.EnBreak.Clear();
            return res;
        }

        public void Dispose()
        {
            AttackerId = default;
            DefenseId = default;
            HitInfo = default;
            ConfigAttackInfo = null;
            DamagePercentage = default;
            DamagePercentageRatio = default;
            StrikeType = default;
            EnBreak.Clear();
            AttackType = default;
            DamageExtra = default;
            BonusCritical = default;
            BonusCriticalHurt = default;
            IgnoreLevelDiff = default;
            TrueDamage = default;
            IsEffective = default;
            FinalRealDamage = default;
        }
    }
}