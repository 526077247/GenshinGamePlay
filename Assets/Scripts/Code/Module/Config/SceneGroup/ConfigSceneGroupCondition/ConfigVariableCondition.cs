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
        public BaseSceneGroupValue LeftValue;
        [NinoMember(2)]
        [Tooltip(SceneGroupTooltips.CompareMode)]
        [OnValueChanged("@"+nameof(CheckModeType)+"("+nameof(RightValue)+","+nameof(Mode)+")")] 
        public CompareMode Mode;
        [NinoMember(3)]
        [LabelText("右值")] 
        public float RightValue;

#if UNITY_EDITOR
        protected override bool CheckModeType<T>(T t, CompareMode mode)
        {
            if (!base.CheckModeType(t, mode))
            {
                this.Mode = CompareMode.Equal;
                return false;
            }

            return true;
        }
#endif
        
        public sealed override bool IsMatch(IEventBase obj, SceneGroup sceneGroup)
        {
            var valLeft = LeftValue.Resolve(obj, sceneGroup.Variable);
            return IsMatch(RightValue, valLeft, Mode);
        }
    }
}