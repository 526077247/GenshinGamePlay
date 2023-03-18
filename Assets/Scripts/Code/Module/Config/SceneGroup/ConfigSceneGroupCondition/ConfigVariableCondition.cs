using System;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [TriggerType]
    [LabelText("变量值")]
    [NinoSerialize]
    public partial class ConfigVariableCondition : ConfigSceneGroupCondition
    {
        [NinoMember(1)]
        [LabelText("左值")] 
        public BaseSceneGroupValue leftValue;
        [NinoMember(2)]
        [Tooltip(SceneGroupTooltips.CompareMode)] [OnValueChanged("@CheckModeType(rightValue,mode)")]
        public CompareMode mode;
        [NinoMember(3)]
        [LabelText("右值")] 
        public float rightValue;

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
        
        public sealed override bool IsMatch(IEventBase obj, SceneGroup sceneGroup)
        {
            var valLeft = leftValue.Resolve(obj, sceneGroup.variable);
            return IsMatch(rightValue, valLeft, mode);
        }
    }
}