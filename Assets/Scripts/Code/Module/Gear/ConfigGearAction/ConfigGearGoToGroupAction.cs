using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// gear调整group进度,只对非randGroup有效
    /// </summary>
    [Serializable][LabelText("跳转到其他Group")]
    public class ConfigGearGoToGroupAction:ConfigGearAction
    {
        [SerializeField][LabelText("要跳转的组Id")]
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