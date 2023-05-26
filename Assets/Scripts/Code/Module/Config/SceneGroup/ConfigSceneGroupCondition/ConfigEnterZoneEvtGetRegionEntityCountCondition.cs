using System;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [LabelText("进入触发区域的指定类型实体的数量")]
    [TriggerType(typeof(ConfigEnterZoneEventTrigger))]
    [NinoSerialize]
    public partial class ConfigEnterZoneEvtGetRegionEntityCountCondition : ConfigSceneGroupCondition<EnterZoneEvent>
    {
        [Tooltip(SceneGroupTooltips.CompareMode)][OnValueChanged("@"+nameof(CheckModeType)+"("+nameof(Value)+","+nameof(Mode)+")")] 
        [NinoMember(1)]
        public CompareMode Mode;
        [NinoMember(2)]
        public EntityType Type;
        [NinoMember(3)]
        public int Value;
        public override bool IsMatch(EnterZoneEvent obj,SceneGroup sceneGroup)
        {
            var zone = sceneGroup.Parent.Get<Zone>(obj.ZoneEntityId)?.GetComponent<SceneGroupZoneComponent>();
            if (zone != null)
            {
                return IsMatch(Value, zone.GetRegionEntityCount(Type), Mode);
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