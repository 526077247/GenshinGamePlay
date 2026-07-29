using System;
using ProtoBuf;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    
    [LabelText("附加Suite")]
    [ProtoContract]
    public partial class ConfigSceneGroupAddExtraSuiteAction : ConfigSceneGroupAction
    {
        [ProtoMember(10)]
        [LabelText("要附加的阶段id")]
#if UNITY_EDITOR
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetSceneGroupSuiteIds)+"()",AppendNextDrawer = true)]
#endif
        public int SuiteId;

        protected override void Execute(IEventBase evt, SceneGroup aimSceneGroup, SceneGroup fromSceneGroup)
        {
            aimSceneGroup.AddExtraSuite(SuiteId);
        }
    }
}