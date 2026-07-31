using System;
using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif
using UnityEngine;

namespace TaoTie
{
    [LabelText("移除附加Suite")]
    [ProtoContract]
    public partial class ConfigSceneGroupRemoveExtraSuiteAction : ConfigSceneGroupAction
    {
        [ProtoMember(10)]
        [LabelText("要移除的阶段id")]
#if UNITY_EDITOR
        [ValueDropdown("@"+ nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetSceneGroupSuiteIds)+"()",AppendNextDrawer = true)]
#endif
        public int SuiteId;
        
        protected override void Execute(IEventBase evt, SceneGroup aimSceneGroup, SceneGroup fromSceneGroup)
        {
            aimSceneGroup.RemoveExtraSuite(SuiteId);
        }
    }
}