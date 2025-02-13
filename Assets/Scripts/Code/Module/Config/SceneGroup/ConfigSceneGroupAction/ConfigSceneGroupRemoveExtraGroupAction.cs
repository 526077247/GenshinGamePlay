using System;
using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [LabelText("移除附加group")]
    [NinoType(false)]
    public partial class ConfigSceneGroupRemoveExtraGroupAction : ConfigSceneGroupAction
    {
        [NinoMember(10)]
#if UNITY_EDITOR
        [ValueDropdown("@"+ nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetSceneGroupSuiteIds)+"()",AppendNextDrawer = true)]
#endif
        public int GroupId;
        
        protected override void Execute(IEventBase evt, SceneGroup aimSceneGroup, SceneGroup fromSceneGroup)
        {
            aimSceneGroup.RemoveExtraGroup(GroupId);
        }
    }
}