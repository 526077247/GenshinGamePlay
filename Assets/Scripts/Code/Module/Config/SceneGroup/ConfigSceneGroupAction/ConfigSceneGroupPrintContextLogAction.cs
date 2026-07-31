using System;
using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 打印log
    /// </summary>
    [LabelText("打印log")]
    [ProtoContract]
    public partial class ConfigSceneGroupPrintContextLogAction : ConfigSceneGroupAction
    {
        [ProtoMember(10)]
        public string Content;
        
        protected override void Execute(IEventBase evt, SceneGroup aimSceneGroup, SceneGroup fromSceneGroup)
        {
            Log.Info(Content + "\r\n" + LitJson.JsonMapper.ToJson(evt));
        }
    }
}