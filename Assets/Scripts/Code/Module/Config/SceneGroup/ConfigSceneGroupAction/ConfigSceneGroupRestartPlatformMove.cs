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
    [LabelText("重设寻路路径")]
    [ProtoContract]
    public partial class ConfigSceneGroupRestartPlatformMove : ConfigSceneGroupAction
    {
        [ProtoMember(10)]
#if UNITY_EDITOR
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetSceneGroupActorIds)+"()",AppendNextDrawer = true)]
#endif
        public int ActorId;
        
        [ProtoMember(11)]
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