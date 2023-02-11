using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [Serializable]
    [TriggerType][LabelText("Gear当前剩余怪物数量")]
    public class ConfigGetGroupMonsterCountCondition : ConfigGearCondition
    {
        [Tooltip(GearTooltips.CompareMode)] [OnValueChanged("@CheckModeType(value,mode)")] [SerializeField]
        public CompareMode mode;
        
        [SerializeField] public int value;
        public override bool IsMatch(IEventBase obj,Gear gear)
        {
            var count = gear.GetGroupMonsterCount();
            // NLog.Info(LogConst.NGear, $"do condition: ConfigGetGroupMonsterCountCondition {_value} {count} {_mode}");
            return IsMatch(value, count, mode);
        }
#if UNITY_EDITOR
        protected override bool CheckModeType<T>(T t, CompareMode mode)
        {
            if (!base.CheckModeType(t, mode))
            {
                this.mode = CompareMode.Equal;
                return false;
            }

            return true;
        }
#endif
    }
}