using System;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [LabelText("重设寻路路径")]
    [NinoSerialize]
    public partial class ConfigSceneGroupRestartPlatformMove : ConfigSceneGroupAction
    {
        [NinoMember(10)]
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetSceneGroupActorIds)+"()",AppendNextDrawer = true)]
        public int ActorId;
        
        [NinoMember(11)]
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetSceneGroupRouteIds)+"()",AppendNextDrawer = true)]
        public int RouteId;
        protected override void Execute(IEventBase evt, SceneGroup aimSceneGroup, SceneGroup fromSceneGroup)
        {
            if (aimSceneGroup.TryGetActorEntity(ActorId, out var entityId))
            {
                if(aimSceneGroup.TryGetRoute(RouteId,out var route))
                {
                    var gadget = aimSceneGroup.Parent.Get<Entity>(entityId).GetComponent<GadgetComponent>();
                    var pmc = gadget?.GetComponent<PlatformMoveComponent>();
                    pmc?.SetRoute(route,0);
                }
              
            }
        }
    }
}