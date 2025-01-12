using System.Collections.Generic;
using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigAttackInfo
    {
        [NinoMember(1)][LabelText("攻击标识")][Tooltip("攻击标识，其他地方可通过这个标记筛选过滤")]
        public string AttackTag;
        [NinoMember(2)][LabelText("衰减类型标记")]
        public string AttenuationTag;
        [NinoMember(3)][LabelText("衰减分组")]
        public string AttenuationGroup;
        [NinoMember(4)][LabelText("伤害数据")][NotNull]
        public ConfigAttackProperty AttackProperty;
        [NinoMember(5)][LabelText("默认打击数据")][NotNull]
        public ConfigHitPattern HitPattern;
        [NinoMember(6)][LabelText("打击数据")]
        public Dictionary<HitBoxType, ConfigHitPattern> HitPatternOverwrite = new();
        [NinoMember(7)][LabelText("强制抖动摄像机")]
        public bool ForceCameraShake;
        [NinoMember(8)][LabelText("抖动摄像机参数")][ShowIf(nameof(ForceCameraShake))][NotNull]
        public ConfigCameraShake CameraShake;
        [NinoMember(9)][LabelText("子弹衰减")]
        public ConfigBulletWane BulletWane;
    }
}