using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [Serializable]
    [LabelText("附加group")]
    public class ConfigGearAddExtraGroupAction : ConfigGearAction
    {

        [SerializeField][LabelText("要附加的组id")]
        public int groupId;

        protected override void Execute(IEventBase evt, Gear aimGear, Gear fromGear)
        {
            aimGear.AddExtraGroup(groupId);
        }
    }
}