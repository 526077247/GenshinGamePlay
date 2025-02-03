using System;
using DaGenGraph;
using Nino.Core;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace TaoTie
{
    [LabelText("判断节点")]
    [NinoType(false)]
    public class ConfigSceneGroupNormalConditionAction: ConfigSceneGroupConditionAction
    {
        [NinoMember(10)]
        [LabelText("条件")][DrawIgnore]
#if UNITY_EDITOR
        [TypeFilter("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetFilteredConditionTypeList)+"("+nameof(HandleType)+")")]
#endif
        public ConfigSceneGroupCondition Conditions;
        
        protected override void Execute(IEventBase evt, SceneGroup aimSceneGroup, SceneGroup fromSceneGroup)
        {
            bool isSuc = Conditions.IsMatch(evt, aimSceneGroup);
            if (isSuc)
            {
                for (int i = 0; i < (Success == null ? 0 : Success.Length); i++)
                {
                    Success[i]?.ExecuteAction(evt, aimSceneGroup,fromSceneGroup);
                }
            }
            else
            {
                for (int i = 0; i < (Fail == null ? 0 : Fail.Length); i++)
                {
                    Fail[i]?.ExecuteAction(evt, aimSceneGroup,fromSceneGroup);
                }
            }
        }
    }
}