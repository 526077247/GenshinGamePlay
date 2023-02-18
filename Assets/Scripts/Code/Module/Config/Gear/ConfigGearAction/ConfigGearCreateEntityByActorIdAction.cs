using System;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [LabelText("通过ActorId创建实体")]
    [NinoSerialize]
    public partial class ConfigGearCreateEntityByActorIdAction : ConfigGearAction
    {
        [NinoMember(10)]
        [ValueDropdown("@OdinDropdownHelper.GetGearActorIds()")]
        public int actorId;
        
        protected override void Execute(IEventBase evt, Gear aimGear, Gear fromGear)
        {
            aimGear.CreateActor(actorId);
        }
    }
}