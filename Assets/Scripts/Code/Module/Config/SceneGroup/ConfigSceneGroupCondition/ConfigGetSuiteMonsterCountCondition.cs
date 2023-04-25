using System;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    
    [TriggerType]
    [LabelText("SceneGroup当前剩余怪物数量")]
    [NinoSerialize]
    public partial class ConfigGetSuiteMonsterCountCondition : ConfigSceneGroupCondition
    {
        [NinoMember(1)]
        [Tooltip(SceneGroupTooltips.CompareMode)] [OnValueChanged("@CheckModeType(Value,Mode)")]
        public CompareMode Mode;
        [NinoMember(2)]
        public int Value;
        public override bool IsMatch(IEventBase obj, SceneGroup sceneGroup)
        {
            var count = sceneGroup.GetSuiteMonsterCount();
            return IsMatch(Value, count, Mode);
        }
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
    }
}