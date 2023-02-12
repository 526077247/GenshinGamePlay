using System;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [LabelText("玩家进入触发区域")]
    [TriggerType(typeof(ConfigEnterZoneEventTrigger))]
    [NinoSerialize]
    public partial class ConfigEnterZoneEvtGetIsMyCondition : ConfigGearCondition<EnterZoneEvent>
    {

        public override bool IsMatch(EnterZoneEvent obj, Gear gear)
        {
            var scene = SceneManager.Instance.CurrentScene as BaseMapScene;
            if (scene != null)
                return obj.EntityId == scene.MyId;
            return false;
        }
#if UNITY_EDITOR
        protected override bool CheckModeType<T>(T t, CompareMode mode)
        {
            if (!base.CheckModeType(t, mode))
            {
                return false;
            }

            return true;
        }
#endif
    }
}