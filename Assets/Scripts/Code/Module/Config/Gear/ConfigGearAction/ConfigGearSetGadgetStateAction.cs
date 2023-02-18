using System;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [LabelText("修改GadgetState状态")]
    [NinoSerialize]
    public partial class ConfigGearSetGadgetStateAction : ConfigGearAction
    {
        [NinoMember(10)]
        [ValueDropdown("@OdinDropdownHelper.GetGearActorIds()")]
        public int actorId;
        [NinoMember(11)]
        public GadgetState state;
        protected override void Execute(IEventBase evt, Gear aimGear, Gear fromGear)
        {
            if (aimGear.TryGetActorEntity(actorId, out var entityId))
            {
                var gadget = aimGear.Parent.Get<Entity>(entityId).GetComponent<GadgetComponent>();
                if (gadget != null)
                {
                    gadget.SetGadgetState(state);
                }
            }
        }
    }
}