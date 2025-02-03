using System;
using DaGenGraph;
using Nino.Core;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
namespace TaoTie
{
    [NinoType(false)]
    public abstract class ConfigSceneGroupLogicConditionAction:ConfigSceneGroupConditionAction
    {
        [NinoMember(10)]
        [LabelText("条件")][DrawIgnore]
#if UNITY_EDITOR
        [TypeFilter("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetFilteredConditionTypeList)+"("+nameof(HandleType)+")")]
#endif
        public ConfigSceneGroupCondition[] Conditions;
    }
}