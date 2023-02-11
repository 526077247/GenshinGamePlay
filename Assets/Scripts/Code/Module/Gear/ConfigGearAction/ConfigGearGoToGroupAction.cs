using System;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// gear调整group进度,只对非randGroup有效
    /// </summary>
    [LabelText("跳转到其他Group")]
    [NinoSerialize]
    public partial class ConfigGearGoToGroupAction:ConfigGearAction
    {
        [LabelText("要跳转的组Id")]
        [ValueDropdown("@OdinDropdownHelper.GetGearGroupIds()")]
        [NinoMember(10)]
        public int groupId;
        protected override void Execute(IEventBase evt, Gear aimGear, Gear fromGear)
        {
            if (!aimGear.config.randGroup)
            {
                aimGear.ChangeGroup(groupId);
            }
            else
            {
                Log.Error("gear调整group进度,只对非randGroup有效");
            }
        }
    }
}