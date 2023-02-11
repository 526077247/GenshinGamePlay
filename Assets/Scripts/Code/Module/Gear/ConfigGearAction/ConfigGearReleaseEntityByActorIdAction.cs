using System;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [LabelText("通过ActorId释放实体")]
    [NinoSerialize]
    public class ConfigGearReleaseEntityByActorIdAction:ConfigGearAction
    {
        [NinoMember(10)]
        [ValueDropdown("@OdinDropdownHelper.GetGearActorIds()")]
        public int actorId;
        
        protected override void Execute(IEventBase evt, Gear aimGear, Gear fromGear)
        {
            if(aimGear.TryGetActorEntity(actorId,out long eid))
            {
                aimGear.Parent.Remove(eid);
            }
        }
    }
}