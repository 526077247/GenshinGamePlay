using System;

using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [Serializable][LabelText("移除附加group")]
    public class ConfigGearRemoveExtraGroupAction : ConfigGearAction
    {
        [SerializeField]
        public int groupId;
    }
}