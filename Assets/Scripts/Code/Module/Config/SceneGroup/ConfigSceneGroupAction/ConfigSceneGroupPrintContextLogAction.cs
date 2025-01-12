using System;
using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 打印log
    /// </summary>
    [LabelText("打印log")]
    [NinoType(false)]
    public partial class ConfigSceneGroupPrintContextLogAction : ConfigSceneGroupAction
    {
        [NinoMember(10)]
        public string Content;
        
        protected override void Execute(IEventBase evt, SceneGroup aimSceneGroup, SceneGroup fromSceneGroup)
        {
            Log.Info(Content + "\r\n" + LitJson.JsonMapper.ToJson(evt));
        }
    }
}