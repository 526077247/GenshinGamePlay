using System.Collections.Generic;
using Nino.Serialization;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public class ConfigAttackInfo
    {
        [NinoMember(1)][Tooltip("攻击标识，其他地方可通过这个标记筛选过滤")]
        public string AttackTag;
        [NinoMember(2)][Tooltip("衰减类型标记")]
        public string AttenuationTag;
        [NinoMember(3)][Tooltip("衰减分组")]
        public string AttenuationGroup;
        [NinoMember(4)][Tooltip("伤害数据")] [NotNull]
        public ConfigAttackProperty AttackProperty;
        [NinoMember(5)][Tooltip("打击数据")]
        public Dictionary<HitBoxType, ConfigHitPattern> HitPattern;
        [NinoMember(6)][Tooltip("强制抖动摄像机")]
        public bool ForceCameraShake;
        [NinoMember(7)][Tooltip("抖动摄像机参数")]
        public ConfigCameraShake CameraShake;
        [NinoMember(8)][Tooltip("子弹衰减")]
        public ConfigBulletWane BulletWane;
    }
}