using System;
using System.Collections.Generic;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 区域
    /// </summary>
    public abstract partial class ConfigSceneGroupZone
    {
        
        #if UNITY_EDITOR
        [NinoMember(0)][LabelText("策划备注")][PropertyOrder(int.MinValue+1)]
        public string Remarks;
        #endif
        [NinoMember(1)][PropertyOrder(int.MinValue)]
        public int LocalId;
        [NinoMember(2)]
        public Vector3 Position;
        [NinoMember(3)][LabelText("是否是相对坐标、方向")]
        public bool IsLocal = true;
        public abstract Zone CreateZone(SceneGroup sceneGroup);
    }
}