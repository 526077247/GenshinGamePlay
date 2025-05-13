using System;
using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [LabelText("重设寻路路径")]
    [NinoType(false)]
    public partial class ConfigSceneGroupRestartPlatformMove : ConfigSceneGroupAction
    {
        [NinoMember(10)]
#if UNITY_EDITOR
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetSceneGroupActorIds)+"()",AppendNextDrawer = true)]
#endif
        public int ActorId;
        
        [NinoMember(11)]
#if UNITY_EDITOR
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetSceneGroupRouteIds)+"()",AppendNextDrawer = true)]
#endif
        public int RouteId;
        protected override void Execute(IEventBase evt, SceneGroup aimSceneGroup, SceneGroup fromSceneGroup)
        {
            if (aimSceneGroup.TryGetActorEntity(ActorId, out var entityId))
            {
                if(aimSceneGroup.TryGetRoute(RouteId,out var route))
                {
                    var gadget = aimSceneGroup.Parent.Get<Entity>(entityId).GetComponent<GadgetComponent>();
                    var pmc = gadget?.GetComponent<MoveComponent>();
                    pmc?.ChangeStrategy(new ConfigPlatformMove()
                    {
                        Route = route,
                        Delay = 0
                    });
                }
              
            }
        }
    }
}