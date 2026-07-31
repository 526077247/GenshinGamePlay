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
    [LabelText("当关卡的变量改变之后")]
    [ProtoContract]
    public partial class ConfigVariableChangeEventTrigger : ConfigSceneGroupTrigger<VariableChangeEvent>
    {
        [ProtoMember(5)][LabelText("变量")]
        public string Key;

        protected override bool CheckCondition(SceneGroup sceneGroup, VariableChangeEvent evt)
        {
            return Key == evt.Key;
        } 
    }
}