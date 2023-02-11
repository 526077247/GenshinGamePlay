using System;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    
    [LabelText("附加group")]
    [NinoSerialize]
    public class ConfigGearAddExtraGroupAction : ConfigGearAction
    {
        [NinoMember(10)]
        [LabelText("要附加的组id")][ValueDropdown("@OdinDropdownHelper.GetGearGroupIds()")]
        public int groupId;

        protected override void Execute(IEventBase evt, Gear aimGear, Gear fromGear)
        {
            aimGear.AddExtraGroup(groupId);
        }
    }
}