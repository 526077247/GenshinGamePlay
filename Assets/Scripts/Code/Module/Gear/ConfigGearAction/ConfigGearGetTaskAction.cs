using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [Serializable][LabelText("接受任务")]
    [TriggerType(typeof(ConfigEnterZoneGearTrigger))]
    public class ConfigGearGetTaskAction: ConfigGearAction
    {
        [SerializeField]
        public int taskId;
    }
}