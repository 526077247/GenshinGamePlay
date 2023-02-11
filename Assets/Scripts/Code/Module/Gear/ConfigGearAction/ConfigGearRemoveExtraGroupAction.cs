using System;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [LabelText("移除附加group")]
    [NinoSerialize]
    public partial class ConfigGearRemoveExtraGroupAction : ConfigGearAction
    {
        [NinoMember(10)]
        [ValueDropdown("@OdinDropdownHelper.GetGearGroupIds()")]
        public int groupId;
        
        protected override void Execute(IEventBase evt, Gear aimGear, Gear fromGear)
        {
            aimGear.RemoveExtraGroup(groupId);
        }
    }
}