using System;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [LabelText("进入触发区域的指定类型实体的数量")]
    [TriggerType(typeof(ConfigExitZoneEventTrigger))]
    [NinoSerialize]
    public partial class ConfigExitZoneEvtGetRegionEntityCountCondition : ConfigSceneGroupCondition<ExitZoneEvent>
    {
        [Tooltip(SceneGroupTooltips.CompareMode)] [OnValueChanged("@CheckModeType(value,mode)")]
        [NinoMember(1)]
        public CompareMode mode;
        [NinoMember(2)]
        public EntityType type;
        [NinoMember(3)]
        public int value;
        public override bool IsMatch(ExitZoneEvent obj,SceneGroup sceneGroup)
        {
            var zone = sceneGroup.Parent.Get<Zone>(obj.ZoneEntityId)?.GetComponent<SceneGroupZoneComponent>();
            if (zone != null)
            {
                return IsMatch(value, zone.GetRegionEntityCount(type), mode);
            }
            return false;
        }
#if UNITY_EDITOR
        protected override bool CheckModeType<T>(T t, CompareMode mode)
        {
            if (!base.CheckModeType(t, mode))
            {
                mode = CompareMode.Equal;
                return false;
            }

            return true;
        }
#endif
    }
}