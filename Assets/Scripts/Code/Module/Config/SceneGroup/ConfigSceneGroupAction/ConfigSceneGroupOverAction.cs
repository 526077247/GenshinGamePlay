using System;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// SceneGroup结束
    /// </summary>
    [LabelText("结束并销毁该SceneGroup")]
    [NinoSerialize]
    public partial class ConfigSceneGroupOverAction : ConfigSceneGroupAction
    {

        protected override void Execute(IEventBase evt, SceneGroup aimSceneGroup, SceneGroup fromSceneGroup)
        {
            aimSceneGroup.Dispose();
        }
    }
}