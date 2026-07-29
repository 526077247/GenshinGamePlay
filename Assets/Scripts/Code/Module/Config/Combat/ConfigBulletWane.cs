using ProtoBuf;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 子弹衰减
    /// </summary>
    [ProtoContract]
    public partial class ConfigBulletWane
    {
        [ProtoMember(1)][LabelText("衰减开始生效时间")][Tooltip("表示从Bullet创生之后这么久，开始套用衰减规则")]
        public float WaneDelay;
        [ProtoMember(2)][LabelText("伤害衰减周期")][Tooltip("伤害衰减周期，每过这么久，进行一次伤害衰减")]
        public float DamageWaneInterval;
        [ProtoMember(3)][LabelText("每次进行伤害衰减时衰减比例")][Tooltip("每次进行伤害衰减时，按照DamageWaneRatio的比例，在damagePercentage上进行【扣除】，最多扣除到0")]
        public float DamageWaneRatio;
        [ProtoMember(4)][LabelText("伤害衰减最低比例")][Tooltip("每次进行伤害衰减时，最低比例")]
        public float DamageWaneMinRatio;
        [ProtoMember(5)][LabelText("元素衰减周期")][Tooltip("元素衰减周期，每过这么久，进行一次元素衰减")]
        public float ElementDurabilityWaneInterval;
        [ProtoMember(6)][LabelText("每次进行元素衰减时衰减比例")]
        public float ElementDurabilityWaneRatio;
        [ProtoMember(7)][LabelText("元素衰减比例最低值")][Tooltip("每次进行元素衰减时，最低比例")]
        public float ElementDurabilityWaneMinRatio;
        [ProtoMember(8)][LabelText("受击等级衰减周期")][Tooltip("受击等级衰减周期，每过这么久，进行一次受击等级衰减，每次将当前受击等级往下降一级：Air→Heavy→Light→Shake→Mute")]
        public float HitLevelWaneInterval;
        [ProtoMember(9)][LabelText("受击等级衰减到的最低等级")][Tooltip("受击等级最少衰减为这个等级。如果原本就不高于这个等级，则不予衰减。")]
        public HitLevel HitLevelWaneMin;
    }
}