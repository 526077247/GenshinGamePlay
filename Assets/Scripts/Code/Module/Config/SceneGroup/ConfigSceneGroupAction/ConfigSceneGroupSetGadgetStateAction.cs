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
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetSceneGroupActorIds)+"()",AppendNextDrawer = true)]
        public int ActorId;
        [NinoMember(11)]
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetGadgetState)+"()")]
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