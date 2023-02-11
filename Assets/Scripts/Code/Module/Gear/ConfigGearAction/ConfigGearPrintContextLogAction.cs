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
        
        protected override void Execute(IEventBase evt, Gear aimGear, Gear fromGear)
        {
            Log.Info(content + "\r\n" + LitJson.JsonMapper.ToJson(evt));
        }
    }
}