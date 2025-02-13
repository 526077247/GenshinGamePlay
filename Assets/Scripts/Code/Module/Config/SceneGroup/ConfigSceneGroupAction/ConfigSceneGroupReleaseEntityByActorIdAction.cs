using System;
using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [LabelText("通过ActorId释放实体")]
    [NinoType(false)]
    public partial class ConfigSceneGroupReleaseEntityByActorIdAction:ConfigSceneGroupAction
    {
        [NinoMember(10)]
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