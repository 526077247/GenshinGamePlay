using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TaoTie
{
    public static class AttackHelper
    {
        public static readonly EntityType[] ActorEntityType = new EntityType[]
            {EntityType.Avatar, EntityType.Monster, EntityType.Gadget};
        public static readonly EntityType[] AvatarEntityType = new EntityType[]
            {EntityType.Avatar};
        /// <summary>
        /// 检查是否目标
        /// </summary>
        /// <param name="self"></param>
        /// <param name="target"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool CheckIsTarget(Entity self, Entity target, TargetType type)
        {
            if(self == null || target == null || self.IsDispose || target.IsDispose)
                return false;
            if (type == TargetType.Self && self.Id != target.Id)
                return false;
            if (type == TargetType.AllExceptSelf && self.Id == target.Id)
                return false;
            if (type == TargetType.Enemy && !CheckIsEnemy(self, target))
                return false;
            if (type == TargetType.SelfCamp && !CheckIsCamp(self, target))
                return false;
            if (type == TargetType.Alliance && !CheckIsAlliance(self, target))
                return false;
            return true;
        }
        /// <summary>
        /// 检查是否是敌人
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool CheckIsEnemy(Entity actor, Entity other)
        {
            if(actor == null || other == null || actor.IsDispose || other.IsDispose)
                return false;
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
            if(actor == null || other == null || actor.IsDispose || other.IsDispose)
                return false;
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
            if(actor == null || other == null || actor.IsDispose || other.IsDispose)
                return false;
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
            if (u == null || u.ConfigActor?.Common == null) return 0;
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
            if (!combatD.CanBeHit) return;
            //非循环命中模式,同一攻击实例对同一目标只结算一次(攻击实例=每次施加的Modifier)
            if (result.HitPattern != null && !result.HitPattern.Recurring && result.Id != 0)
            {
                if (combatD.HasAlreadyHit(result.Id))
                {
                    result.IsEffective = false;
                    return;
                }
                combatD.RecordHit(result.Id);
            }
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
            result.TrueDamage = result.ConfigAttackInfo.AttackProperty.TrueDamage;
            result.IgnoreLevelDiff = result.ConfigAttackInfo.AttackProperty.IgnoreLevelDiff;
            result.StrikeType = result.ConfigAttackInfo.AttackProperty.StrikeType;
            result.AttackType = result.ConfigAttackInfo.AttackProperty.AttackType;

            // 击退数据一律取自 HitPattern：抗性缩放在受击方的 MoveComponent.ApplyHitImpulse 里按
            // HitImpulseType 查受击方配置，攻击侧不再维护第二套「覆盖值」。
            var impulse = result.HitPattern;
            result.HitImpulseType = impulse?.HitImpulseType;
            if (impulse != null)
            {
                result.HitLevel = impulse.HitLevel;
                result.HitImpulseX = impulse.HitImpulseX.Resolve(attacker, ability);
                result.HitImpulseY = impulse.HitImpulseY.Resolve(attacker, ability);
                result.RetreatDir = TargetHelper.ResolveTarget(attacker, result, defence, impulse.RetreatType);
            }

            combatA.BeforeAttack(result, combatD);
            if (!result.IsEffective) return; //被取消
            combatD.BeforeBeAttack(result, combatA);
            if (!result.IsEffective) return; //被取消

            result.FinalRealDamage = 0;

            //子弹，按飞行时间衰减伤害与受击等级
            if (result.IsBullet) ApplyBulletWane(result);

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

            //命中特效先出，再扣血（配置了 HitPattern.OnHitEffectName 才会走到这里）
            if (!string.IsNullOrEmpty(result.HitPattern?.OnHitEffectName))
            {
                SpawnHitEffect(em, defence, result);
            }

            //修改血量
            var finalHp = numD.GetAsInt(NumericType.HpBase) - result.FinalRealDamage;
            if (finalHp < 0) finalHp = 0;
            numD.Set(NumericType.HpBase, finalHp);
            //破霸体（已打死就不再削韧，避免给正在销毁的尸体广播数值变化）
            if (finalHp > 0)
                ApplyNumericBreak(numD, attacker, ability,
                    result.ConfigAttackInfo.AttackProperty, result.HitInfo.HitBoxType);

            combatD.AfterBeAttack(result, combatA);
            combatA.AfterAttack(result, combatD);

            if (finalHp == 0)
            {
                combatD.DoKill(combatA.Id, DieStateFlag.None); //todo: State
            }
        }

        /// <summary>
        /// 削韧结算：按 <see cref="ConfigAttackProperty.NumericBreak"/> 逐条扣对应数值属性的 base 段。
        ///
        /// 这里只把数值改对，<b>不负责破韧之后发生什么</b>：「归零 → 眩晕 / 易伤 / 特效」
        /// 由挂在单位常驻 Ability 上的 <see cref="ConfigDoActionOnNumericChangeMixin"/> 配置出来
        /// （监听某个属性跨入 0）。不同单位想怎么反应就怎么配，不必每加一种反应就回来改这里。
        ///
        /// 四条规则，缺一条行为就会歪：
        /// 1. <b>只认属性表的组首名</b>（EN → 1012，再换算到 base 段 10121）。
        ///    <c>GetKey("ENBase")</c> 也能返回 10121，<c>GetKey("MaxEN")</c> 会返回 1014，
        ///    这两种都必须拒绝：前者是段位歧义，后者写进去等于永久改上限。
        /// 2. <b>已归零就不再扣</b> —— 天然构成"破韧免疫窗口"，不需要额外的无敌计时器；
        ///    等 <see cref="NumericSystem"/> 的通用回复循环把韧性回满，下一轮自然可以再破。
        ///    若不加这一条，恢复量会被破韧期间的后续伤害持续抵消，眩晕时间被无限拉长。
        ///    这个判断是按属性独立的：EN 破了不影响还满着的 Shield。
        /// 3. <b>必须 clamp 到 0</b> —— 韧性不是血量，负值没有意义；一旦出现负数，
        ///    "扣前值 &lt;= 0 即已在破韧中"的判断会一直成立，这个单位就永久不可再破。
        /// 4. <b>整数属性四舍五入，小数属性保留精度</b> —— <see cref="NumericComponent.Set(int,float,bool)"/>
        ///    对整数属性是向零截断，24.5 会写成 24（看起来像少削 1 点，且没有日志能察觉）。
        ///
        /// 剩下的交给事件：<see cref="NumericComponent"/> 在值真的变化时才广播 NumericChangeEvt，
        /// 所以 25 → 0 会触发一次、0 → 0 不广播、恢复期间 0 → 20 又不会命中"跨入 0"，不会重复破。
        /// </summary>
        private static void ApplyNumericBreak(NumericComponent numD, Entity attacker,
                                              ActorAbility ability,
                                              ConfigAttackProperty prop, HitBoxType hitBox)
        {
            var list = prop?.NumericBreak;
            if (list == null || list.Count == 0) return;

            for (int i = 0; i < list.Count; i++)
            {
                var entry = list[i];
                if (entry == null || entry.HitBox != hitBox || entry.Value == null) continue;

                var id = NumericType.GetKey(entry.NumericKey);
                if (id < 0) continue; // 名字不存在，GetKey 内部已 Log.Error
                if (id > NumericType.Max)
                {
                    Log.Error($"NumericBreak 的属性名请填组首名（如 EN），不要填段位：{entry.NumericKey} → {id}");
                    continue;
                }
                if (entry.NumericKey.StartsWith("Max"))
                {
                    Log.Error($"NumericBreak 不能填上限属性，会把上限削掉：{entry.NumericKey}");
                    continue;
                }

                var baseId = id * 10 + 1;
                var cur = numD.GetAsFloat(baseId);
                if (cur <= 0f) continue; // 已在破韧免疫窗口内，不再重复扣

                var value = entry.Value.Resolve(attacker, ability);
                if (value <= 0f) continue; // 负值等于回韧，会让目标永远破不了

                var next = cur - value;
                if (next < 0f) next = 0f;
                if (!numD.IsFloat(baseId)) next = Mathf.RoundToInt(next);

                numD.Set(baseId, next);
            }
        }

        /// <summary>
        /// 命中特效（ConfigHitPattern.OnHitEffectName）：挂到受击者模型的 "Effect" 挂点上。
        ///
        /// 三条约定：
        /// 1. <b>逻辑父必须是受击者</b>：独立创建的 Effect 没有 AttachComponent，
        ///    EffectModelComponent 拿不到"所属角色"，层级抬升量为 0，特效会停在角色骨架平面后面被整个裁掉。
        /// 2. <b>位置取挂点，不取 HitBox 顶面</b>：HitBox 随动作和体型变化，
        ///    同一技能打不同目标时特效会在头顶和脚底之间跳；挂点是美术摆好的固定锚，
        ///    由模型自己决定挂在哪，代码不用再猜。
        /// 3. <b>lifeByOwnerIsAlive = false</b>：不能被受击者"一起走"——这一击正好打死目标时，
        ///    受击者会立刻 Dispose，挂成 lifeByOwnerIsAlive 的子实体连一帧都播不出来。
        ///
        /// 模型上没有 "Effect" 挂点时退回旧逻辑（HitBox 顶面 + 只挂 AttachComponent），不会炸。
        /// </summary>
        private static void SpawnHitEffect(EntityManager em, Entity defender, AttackResult result)
        {
            var effect = em.CreateEntity<Effect, string>(result.HitPattern.OnHitEffectName);
            if (effect == null) return;

            var model = defender.GetComponent<UnitModelComponent>();
            var attachPoint = model?.GetCollectorObj<Transform>("Effect");

            Vector3 pos;
            if (attachPoint != null)
            {
                pos = attachPoint.position;
            }
            else
            {
                // 兜底：没配挂点的模型沿用 HitBox 顶面（碰撞盒顶面比命中点稳，命中点可能落在脚底或侧面）
                pos = result.HitInfo.HitPos;
                var hitBox = model?.GetCollectorObj<Transform>("HitBox");
                var collider = hitBox != null ? hitBox.GetComponentInChildren<Collider>(true) : null;
                if (collider != null)
                {
                    var bounds = collider.bounds;
                    pos = new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
                }
            }

            effect.Position = pos;
            effect.DelayDispose(2000);
            defender.GetOrAddComponent<AttachComponent>().AddChild(effect, false);

            // 物理挂到挂点节点下才会跟随受击者移动/旋转；SetAttachPoint 会把 localPosition 归零，
            // 世界位置仍是上面算的挂点位置，两者一致。它内部要等预制体加载，所以是异步的。
            if (attachPoint != null)
            {
                var effectModel = effect.GetComponent<EffectModelComponent>();
                if (effectModel != null)
                {
                    effectModel.SetAttachPoint(attachPoint).Coroutine();
                }
            }
        }

        /// <summary>
        /// 子弹衰减：按「创生至今」的飞行时间等比扣减伤害、逐级下调受击等级。
        ///
        /// 调用位置很关键 —— 必须在 <see cref="CombatComponent.AfterBeAttack"/> 之前，
        /// 因为受击等级决定「>= Shake 才击退/抖动」的门禁，改晚了这一发就还是按原等级打。
        /// 时停用的是 HitPattern.HitHaltTime，与等级无关，不受影响。
        ///
        /// 时间基准是 <see cref="BulletComponent.CreateTime"/> / <see cref="AttackResult.StartTime"/> 的
        /// 缩放时间：时停期间子弹本身也不动，所以「不衰减」正是预期行为。
        /// </summary>
        private static void ApplyBulletWane(AttackResult result)
        {
            var wane = result.ConfigAttackInfo?.BulletWane;
            if (wane == null) return;

            // 全 0 配置（现存 Test_Bullet 就是这样）必须零副作用，也不能走进除零/除负分支
            if (wane.DamageWaneInterval <= 0f && wane.HitLevelWaneInterval <= 0f) return;
            if (result.StartTime <= 0) return; // 拿不到创生时间（不是真子弹）就不衰减

            long elapsed = GameTimerManager.Instance.GetTimeNow() - result.StartTime;
            if (elapsed <= 0) return;

            float elapsedSec = elapsed * 0.001f;
            if (elapsedSec <= wane.WaneDelay) return; // 还在保护期内，满额结算

            float stepped = elapsedSec - wane.WaneDelay;

            // 伤害：每过一个周期扣一次，复利式 (1-Ratio)^n（字段语义就是「按比例扣除」）
            if (wane.DamageWaneInterval > 0f)
            {
                int steps = (int)(stepped / wane.DamageWaneInterval);
                float ratioEach = Mathf.Clamp01(wane.DamageWaneRatio);
                if (steps > 0 && ratioEach > 0f)
                {
                    float ratio = Mathf.Pow(1f - ratioEach, steps);
                    ratio = Mathf.Max(ratio, Mathf.Clamp01(wane.DamageWaneMinRatio)); // MinRatio 缺省 0 → 允许衰减到 0
                    result.DamagePercentage *= ratio;
                }
            }

            // 受击等级：每过一个周期降一级，不低于 HitLevelWaneMin（原本就不高于它则不衰减）
            if (wane.HitLevelWaneInterval > 0f)
            {
                int steps = (int)(stepped / wane.HitLevelWaneInterval);
                int min = (int)wane.HitLevelWaneMin;
                int lv = (int)result.HitLevel;
                if (steps > 0 && lv > min)
                {
                    lv -= steps;
                    if (lv < min) lv = min;
                    result.HitLevel = (HitLevel)lv;
                }
            }
        }
    }
}