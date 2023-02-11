using System;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 打印log
    /// </summary>
    [LabelText("打印log")]
    [NinoSerialize]
    public class ConfigGearPrintContextLogAction : ConfigGearAction
    {
        [NinoMember(10)]
        public string content;
        
        protected override void Execute(IEventBase evt, Gear aimGear, Gear fromGear)
        {
            Log.Info(content + "\r\n" + LitJson.JsonMapper.ToJson(evt));
        }
    }
}