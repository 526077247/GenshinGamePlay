using System.Collections.Generic;
using ProtoBuf;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigAttackInfo
    {
        [ProtoMember(1)][LabelText("攻击标识")][Tooltip("攻击标识，其他地方可通过这个标记筛选过滤")]
        public string AttackTag;
        [ProtoMember(2)][LabelText("衰减类型标记")]
        public string AttenuationTag;
        [ProtoMember(3)][LabelText("衰减分组")]
        public string AttenuationGroup;
        [ProtoMember(4, IsRequired = true)][LabelText("伤害数据")][NotNull]
        public ConfigAttackProperty AttackProperty = new ConfigAttackProperty();
        [ProtoMember(5, IsRequired = true)][LabelText("默认打击数据")][NotNull]
        public ConfigHitPattern HitPattern = new ConfigHitPattern();
        [ProtoMember(6, IsRequired = true)][LabelText("打击数据")]
        public Dictionary<HitBoxType, ConfigHitPattern> HitPatternOverwrite = new Dictionary<HitBoxType, ConfigHitPattern>();
        [ProtoMember(7)][LabelText("摄像机抖动")]
        public bool ForceCameraShake;
        [ProtoMember(8, IsRequired = true)][LabelText("摄像机抖动参数")][ShowIf(nameof(ForceCameraShake))][NotNull]
        public ConfigCameraShake CameraShake = new ConfigCameraShake();
        [ProtoMember(9)][LabelText("子弹衰减")]
        public ConfigBulletWane BulletWane;
    }
}