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
    [LabelText("通过ActorId释放实体")]
    [ProtoContract]
    public partial class ConfigSceneGroupReleaseEntityByActorIdAction:ConfigSceneGroupAction
    {
        [ProtoMember(10)]
#if UNITY_EDITOR
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetSceneGroupActorIds)+"()", AppendNextDrawer = true)]
#endif
        public int ActorId;
        
        protected override void Execute(IEventBase evt, SceneGroup aimSceneGroup, SceneGroup fromSceneGroup)
        {
            if(aimSceneGroup.TryGetActorEntity(ActorId,out long eid))
            {
                aimSceneGroup.Parent.Remove(eid);
            }
        }
    }
}