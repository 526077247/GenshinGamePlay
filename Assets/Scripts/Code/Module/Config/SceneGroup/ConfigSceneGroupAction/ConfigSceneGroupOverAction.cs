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
    /// SceneGroup结束
    /// </summary>
    [LabelText("结束并销毁该SceneGroup")]
    [ProtoContract]
    public partial class ConfigSceneGroupOverAction : ConfigSceneGroupAction
    {

        protected override void Execute(IEventBase evt, SceneGroup aimSceneGroup, SceneGroup fromSceneGroup)
        {
            aimSceneGroup.Dispose();
        }
    }
}