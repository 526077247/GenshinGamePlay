using System;

using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [TriggerType]
    [Serializable][LabelText("变量值")]
    public class ConfigVariableCondition : ConfigGearCondition
    {
        [SerializeReference] [LabelText("左值")] 
        public AbstractVariableValue leftValue;

        [Tooltip(GearTooltips.CompareMode)] [OnValueChanged("@CheckModeType(rightValue,mode)")] [SerializeField]
        public CompareMode mode;

        [SerializeReference] [LabelText("右值")] public float rightValue;

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
        
        public sealed override bool IsMatch(IEventBase obj, Gear gear)
        {
            var valLeft = leftValue.Resolve(obj, gear.variable);
            return IsMatch(rightValue, valLeft, mode);
        }
    }
}