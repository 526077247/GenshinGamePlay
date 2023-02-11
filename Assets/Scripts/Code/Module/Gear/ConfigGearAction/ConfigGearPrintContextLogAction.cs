using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 打印log
    /// </summary>
    [Serializable][LabelText("打印log")]
    public class ConfigGearPrintContextLogAction : ConfigGearAction
    {
        [SerializeField] public string content;
    }
}