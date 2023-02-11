using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [Serializable][LabelText("当关卡的变量改变之后")]
    public class ConfigVariableChangeGearTrigger : ConfigGearTrigger<VariableChangeEvent>
    {
        [SerializeField] 
        public string key;

    }
}