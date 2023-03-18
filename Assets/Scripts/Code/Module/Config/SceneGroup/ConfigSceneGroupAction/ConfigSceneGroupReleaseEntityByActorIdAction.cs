using System;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [LabelText("通过ActorId释放实体")]
    [NinoSerialize]
    public partial class ConfigSceneGroupReleaseEntityByActorIdAction:ConfigSceneGroupAction
    {
        [NinoMember(10)]
        [ValueDropdown("@OdinDropdownHelper.GetSceneGroupActorIds()")]
        public int actorId;
        
        protected override void Execute(IEventBase evt, SceneGroup aimSceneGroup, SceneGroup fromSceneGroup)
        {
            if(aimSceneGroup.TryGetActorEntity(actorId,out long eid))
            {
                aimSceneGroup.Parent.Remove(eid);
            }
        }
    }
}