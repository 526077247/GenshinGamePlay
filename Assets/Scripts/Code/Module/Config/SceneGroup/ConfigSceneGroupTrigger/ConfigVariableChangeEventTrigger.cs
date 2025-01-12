using System;
using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [LabelText("当关卡的变量改变之后")]
    [NinoType(false)]
    public partial class ConfigVariableChangeEventTrigger : ConfigSceneGroupTrigger<VariableChangeEvent>
    {
        [NinoMember(5)][LabelText("变量")]
        public string Key;

        protected override bool CheckCondition(SceneGroup sceneGroup, VariableChangeEvent evt)
        {
            return Key == evt.Key;
        } 
    }
}