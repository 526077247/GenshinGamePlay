using System;
using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [LabelText("唤醒寻路")]
    [NinoType(false)]
    public partial class ConfigSceneGroupResumePlatformMove : ConfigSceneGroupAction
    {
        [NinoMember(10)]
#if UNITY_EDITOR
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetSceneGroupActorIds)+"()",AppendNextDrawer = true)]
#endif
        public int ActorId;
        protected override void Execute(IEventBase evt, SceneGroup aimSceneGroup, SceneGroup fromSceneGroup)
        {
            if (aimSceneGroup.TryGetActorEntity(ActorId, out var entityId))
            {
                var gadget = aimSceneGroup.Parent.Get<Entity>(entityId).GetComponent<GadgetComponent>();
                var pmc = gadget?.GetComponent<PlatformMoveComponent>();
                pmc?.Resume();
            }
        }
    }
}