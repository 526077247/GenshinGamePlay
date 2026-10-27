using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 子弹衰减：子弹飞得越久，伤害与受击等级越弱。
    ///
    /// 时间单位一律是 <b>秒</b>，注意与 <see cref="ConfigHitPattern.HitHaltTime"/> 的毫秒口径区分开。
    /// 计时基准是子弹的创生时间（<see cref="BulletComponent.CreateTime"/>，缩放时间），
    /// 所以时停期间不衰减 —— 时停时子弹本身也不动，这是预期行为。
    /// </summary>
    [ProtoContract]
    public partial class ConfigBulletWane
    {
        [ProtoMember(1)][LabelText("衰减开始生效时间(s)")][Tooltip("子弹创生后经过这么久才开始套用衰减规则，在此之前按满额结算")]
        public float WaneDelay;
        [ProtoMember(2)][LabelText("伤害衰减周期(s)")][Tooltip("每过这么久扣一次伤害。<=0 表示伤害不衰减")]
        public float DamageWaneInterval;
        [ProtoMember(3)][LabelText("每次伤害衰减比例")][Tooltip("复利式扣除：衰减 n 次后伤害乘 (1-Ratio)^n。<=0 表示伤害不衰减")]
        public float DamageWaneRatio;
        [ProtoMember(4)][LabelText("伤害衰减最低比例")][Tooltip("衰减到的最低倍数，0.2 表示最低保留 20% 伤害。填 0 表示可以一路衰减到 0")]
        public float DamageWaneMinRatio;
        // 5/6/7：原元素耐久衰减（ElementDurabilityWane*）已删除。
        //    工程里没有元素附着/耐久系统，这三个字段全工程零消费方，留着只会让人以为「配了就有效果」。
        //    编号保留不复用。
        [ProtoMember(8)][LabelText("受击等级衰减周期(s)")][Tooltip("每过这么久把受击等级降一级：Air→Heavy→Light→Shake→Mute。<=0 表示等级不衰减")]
        public float HitLevelWaneInterval;
        [ProtoMember(9)][LabelText("受击等级衰减下限")][Tooltip("最多衰减到这个等级；原本就不高于它则不衰减")]
        public HitLevel HitLevelWaneMin;
    }
}
