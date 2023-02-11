using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [Serializable][LabelText("进入触发区域的指定类型实体的数量")]
    [TriggerType(typeof(ConfigExitZoneGearTrigger))]
    public class ConfigExitZoneEvtGetRegionEntityCountCondition : ConfigGearCondition<ExitZoneEvent>
    {
        [Tooltip(GearTooltips.CompareMode)] [OnValueChanged("@CheckModeType(value,mode)")] [SerializeField]
        public CompareMode mode;

        [SerializeField] public EntityType type;
        [SerializeField] public int value;
        public override bool IsMatch(ExitZoneEvent obj,Gear gear)
        {
            var zone = gear.Parent.Get<Zone>(obj.ZoneEntityId)?.GetComponent<GearZoneComponent>();
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