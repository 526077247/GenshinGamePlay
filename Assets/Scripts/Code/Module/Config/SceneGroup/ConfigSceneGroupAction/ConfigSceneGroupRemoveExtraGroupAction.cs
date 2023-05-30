using System;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [LabelText("移除附加group")]
    [NinoSerialize]
    public partial class ConfigSceneGroupRemoveExtraGroupAction : ConfigSceneGroupAction
    {
        [NinoMember(10)]
        [ValueDropdown("@"+ nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetSceneGroupSuiteIds)+"()",AppendNextDrawer = true)]
        public int GroupId;
        
        protected override void Execute(IEventBase evt, SceneGroup aimSceneGroup, SceneGroup fromSceneGroup)
        {
            aimSceneGroup.RemoveExtraGroup(GroupId);
        }
    }
}