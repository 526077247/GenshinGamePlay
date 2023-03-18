using System;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [LabelText("修改GadgetState状态")]
    [NinoSerialize]
    public partial class ConfigSceneGroupSetGadgetStateAction : ConfigSceneGroupAction
    {
        [NinoMember(10)]
        [ValueDropdown("@OdinDropdownHelper.GetSceneGroupActorIds()")]
        public int actorId;
        [NinoMember(11)]
        public GadgetState state;
        protected override void Execute(IEventBase evt, SceneGroup aimSceneGroup, SceneGroup fromSceneGroup)
        {
            if (aimSceneGroup.TryGetActorEntity(actorId, out var entityId))
            {
                var gadget = aimSceneGroup.Parent.Get<Entity>(entityId).GetComponent<GadgetComponent>();
                if (gadget != null)
                {
                    gadget.SetGadgetState(state);
                }
            }
        }
    }
}