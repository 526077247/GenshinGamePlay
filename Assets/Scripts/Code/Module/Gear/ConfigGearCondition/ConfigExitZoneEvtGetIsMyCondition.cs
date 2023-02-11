using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [Serializable][LabelText("玩家进入触发区域")]
    [TriggerType(typeof(ConfigExitZoneGearTrigger))]
    public class ConfigExitZoneEvtGetIsMyCondition : ConfigGearCondition<ExitZoneEvent>
    {
        public override bool IsMatch(ExitZoneEvent obj, Gear gear)
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