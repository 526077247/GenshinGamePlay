using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [Serializable][LabelText("玩家进入触发区域")]
    [TriggerType(typeof(ConfigEnterZoneGearTrigger))]
    public class ConfigEnterZoneEvtGetIsMyCondition : ConfigGearCondition
    {

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