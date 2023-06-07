using System;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 伤害句柄，其他地方不要持有引用
    /// </summary>
    public class AttackResult: IDisposable
    {
        public bool IsBullet;
        public long AttackerId;
        public long DefenseId;
        public HitInfo HitInfo;
        public ConfigAttackInfo ConfigAttackInfo;
        public ConfigHitPattern HitPattern;
        public long StartTime;

        #region 伤害数据
        
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
        public int EnBreak;
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
        
        #endregion

        #region 击退数据

        /// <summary>
        /// 击退力度
        /// </summary>
        public HitLevel HitLevel;

        public float HitImpulseX;

        public float HitImpulseY; 
        /// <summary>
        /// 击退方向
        /// </summary>
        public Vector3 RetreatDir;
        #endregion
        public static AttackResult Create(long attackerId,long defenseId, HitInfo info,ConfigAttackInfo config,bool isBullet = false,long startTime = 0)
        {
            AttackResult res = new AttackResult();
            res.AttackerId = attackerId;
            res.DefenseId = defenseId;
            res.HitInfo = info;
            res.ConfigAttackInfo = config;
            if (config != null)
            {
                config.HitPatternOverwrite?.TryGetValue(info.HitBoxType, out res.HitPattern);
                if(res.HitPattern == null)
                {
                    res.HitPattern = config.HitPattern;
                }
            }
            res.IsEffective = true;
            res.IsBullet = isBullet;
            res.StartTime = startTime;
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
            EnBreak = default;
            AttackType = default;
            DamageExtra = default;
            BonusCritical = default;
            BonusCriticalHurt = default;
            IgnoreLevelDiff = default;
            TrueDamage = default;
            IsEffective = default;
            FinalRealDamage = default;
            HitPattern = default;
            IsBullet = default;
            StartTime = default;
        }
    }
}