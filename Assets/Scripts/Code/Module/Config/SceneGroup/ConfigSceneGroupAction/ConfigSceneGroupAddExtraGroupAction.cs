using System;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    
    [LabelText("附加group")]
    [NinoSerialize]
    public partial class ConfigSceneGroupAddExtraGroupAction : ConfigSceneGroupAction
    {
        [NinoMember(10)]
        [LabelText("要附加的组id")]
#if UNITY_EDITOR
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetSceneGroupSuiteIds)+"()",AppendNextDrawer = true)]
#endif
        public int GroupId;

        protected override void Execute(IEventBase evt, SceneGroup aimSceneGroup, SceneGroup fromSceneGroup)
        {
            aimSceneGroup.AddExtraGroup(GroupId);
        }
    }
}