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
        [LabelText("要附加的组id")][ValueDropdown("@OdinDropdownHelper.GetSceneGroupSuiteIds()")]
        public int groupId;

        protected override void Execute(IEventBase evt, SceneGroup aimSceneGroup, SceneGroup fromSceneGroup)
        {
            aimSceneGroup.AddExtraGroup(groupId);
        }
    }
}