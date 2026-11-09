using System;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 伤害句柄，其他地方不要持有引用
    /// </summary>
    public class AttackResult: IDisposable
    {
        public long Id;
        public bool IsBullet;
        public long AttackerId;
        /// <summary>
        /// 攻击的<b>起点</b>实体 Id。非子弹链路与 AttackerId 相同语义；
        /// 子弹链路下是射手（<see cref="BulletComponent.OwnerId"/>），
        /// 因为此时 AttackerId 可能被解析成子弹自己。
        /// 0 表示未知，<see cref="RetreatType.ByOriginOwner"/> 会退化成 ByAttacker。
        /// </summary>
        public long OriginOwnerId;
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

        /// <summary>
        /// 本次被吸收掉的伤害量（护盾 / 护体值等）。
        ///
        /// <b>「最终伤害」与「实际掉血」是两件事</b>：<see cref="FinalRealDamage"/> 始终是
        /// 这一击打出去的<b>全额</b>，真正扣到 HP 上的是 <c>FinalRealDamage - AbsorbedDamage</c>。
        /// 之所以不把 <see cref="FinalRealDamage"/> 直接改小：飘字、时停、仇恨、受击后钩子
        /// 读的都是它，改小会让「被盾挡住的一击」在系统里变成「没打中」——
        /// 表现为不掉血但也不出伤害字、不触发受击特效、不进入战斗状态。
        /// </summary>
        public int AbsorbedDamage { get; private set; }

        #endregion

        #region 护盾吸收

        private struct AbsorbEntry
        {
            public IDamageAbsorber Source;
            public int Priority;
        }

        /// <summary>
        /// 登记的吸收者。懒创建 —— 没有吸收机制的单位（绝大多数怪物）永远不分配这块内存。
        ///
        /// 容量给到 8：多盾（火 / 冰 / 雷 / 通用 / 心法）同时挂在一个单位上不再是理论情况。
        /// 超出的会被丢弃并告警 —— 静默丢一面盾的表现是「某类伤害突然开始掉血」，极难定位。
        /// </summary>
        private AbsorbEntry[] absorbEntries;
        private int absorbCount;

        /// <summary>
        /// 登记一个吸收者。必须在最终伤害算出<b>之前</b>调用（受击前钩子），
        /// 由 <see cref="AttackHelper.DamageClose"/> 在结算之后统一执行。
        ///
        /// 为什么不在登记时就扣：受击前钩子跑在 206 行，而最终伤害要到 243 行才算出来。
        /// 那里只有未经等级差 / 防御 / 暴击 / 子弹衰减的原料，提前扣只能靠「预演公式」，
        /// 而预演暴击会消耗 <c>Random</c>，把真实的暴击序列也一起改掉。
        /// </summary>
        public void RegisterAbsorb(IDamageAbsorber source, int priority)
        {
            if (source == null) return;
            absorbEntries ??= new AbsorbEntry[8];
            if (absorbCount == absorbEntries.Length)
            {
                Log.Warning($"同一次伤害结算登记的吸收者超过 {absorbEntries.Length} 个，超出的被忽略；" +
                            $"请检查是否有多条 ConfigAbsorbByStateMixin 同时挂在了同一个实体上");
                return;
            }
            absorbEntries[absorbCount++] = new AbsorbEntry { Source = source, Priority = priority };
        }

        /// <summary>
        /// 按优先级从低到高依次吸收，返回扣完盾之后真正落到 HP 上的伤害。
        /// 没有登记任何护盾时原样返回，不产生副作用。
        /// </summary>
        public int ResolveAbsorb(int damage)
        {
            if (absorbCount == 0)
            {
                AbsorbedDamage = 0;
                return damage;
            }

            // 最多 4 项，插入排序最省（不分配、不比较器）
            for (int i = 1; i < absorbCount; i++)
            {
                var cur = absorbEntries[i];
                int j = i - 1;
                while (j >= 0 && absorbEntries[j].Priority > cur.Priority)
                {
                    absorbEntries[j + 1] = absorbEntries[j];
                    j--;
                }
                absorbEntries[j + 1] = cur;
            }

            int left = damage;
            int count = absorbCount;
            absorbCount = 0;
            for (int i = 0; i < count && left > 0; i++)
            {
                int absorbed = absorbEntries[i].Source.AbsorbDamage(left);
                if (absorbed > 0) left -= Mathf.Min(absorbed, left);
            }

            // 吸收者可能在破盾时把宿主 modifier 一起移除（实例已回池），
            // 所以这里只清引用，不得再触碰 Source 的任何成员
            for (int i = 0; i < count; i++) absorbEntries[i] = default;

            AbsorbedDamage = damage - left;
            return left;
        }

        #endregion

        #region 击退数据

        /// <summary>
        /// 击退力度
        /// </summary>
        public HitLevel HitLevel;

        public float HitImpulseX;

        public float HitImpulseY; 
        /// <summary>
        /// 击退类型(作为分类标签,后续用于目标击退抗性过滤)
        /// </summary>
        public string HitImpulseType;
        /// <summary>
        /// 击退方向
        /// </summary>
        public Vector3 RetreatDir;
        #endregion
        public static AttackResult Create(long attackerId,long defenseId, HitInfo info,ConfigAttackInfo config,bool isBullet = false,long startTime = 0,long attackInfoId = 0)
        {
            AttackResult res = new AttackResult();
            res.Id = attackInfoId;//攻击Id
            res.AttackerId = attackerId;
            // 默认「起点 == 攻击者」，子弹链路再按 BulletComponent.OwnerId 覆写成射手
            res.OriginOwnerId = attackerId;
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
            Id = default;
            AttackerId = default;
            OriginOwnerId = default;
            DefenseId = default;
            HitInfo = default;
            ConfigAttackInfo = null;
            DamagePercentage = default;
            DamagePercentageRatio = default;
            StrikeType = default;
            AttackType = default;
            DamageExtra = default;
            BonusCritical = default;
            BonusCriticalHurt = default;
            IgnoreLevelDiff = default;
            TrueDamage = default;
            IsEffective = default;
            IsCritical = default;
            FinalRealDamage = default;
            HitLevel = default;
            HitImpulseX = default;
            HitImpulseY = default;
            HitImpulseType = default;
            RetreatDir = default;
            HitPattern = default;
            IsBullet = default;
            StartTime = default;
            AbsorbedDamage = default;
            absorbCount = default;
            if (absorbEntries != null)
            {
                Array.Clear(absorbEntries, 0, absorbEntries.Length);
                absorbEntries = null;
            }
        }
    }
}