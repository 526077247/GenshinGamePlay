using System;
using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [LabelText("修改GadgetState状态")]
    [NinoType(false)]
    public partial class ConfigSceneGroupSetGadgetStateAction : ConfigSceneGroupAction
    {
        [NinoMember(10)]
#if UNITY_EDITOR
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetSceneGroupActorIds)+"()",AppendNextDrawer = true)]
#endif
        public int ActorId;
        [NinoMember(11)]
#if UNITY_EDITOR
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetGadgetState)+"()")]
#endif
        public GadgetState State;
        protected override void Execute(IEventBase evt, SceneGroup aimSceneGroup, SceneGroup fromSceneGroup)
        {
            if (aimSceneGroup.TryGetActorEntity(ActorId, out var entityId))
            {
                var gadget = aimSceneGroup.Parent.Get<Entity>(entityId).GetComponent<GadgetComponent>();
                if (gadget != null)
                {
                    gadget.SetGadgetState(State);
                }
            }
        }
    }
}