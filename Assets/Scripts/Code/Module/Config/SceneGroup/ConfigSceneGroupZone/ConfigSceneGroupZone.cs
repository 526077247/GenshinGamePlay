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
        [SerializeField] [LabelText("策划备注")][PropertyOrder(int.MinValue+1)]
        private string Remarks;
        #endif
        [NinoMember(1)][PropertyOrder(int.MinValue)]
        public int LocalId;
        [NinoMember(2)]
        public Vector3 Position;

        public abstract Zone CreateZone(SceneGroup sceneGroup);
    }
}