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
    public abstract class ConfigGearZone
    {
        
        #if UNITY_EDITOR
        [LabelText("策划备注")][PropertyOrder(int.MinValue+1)]
        private string remarks;
        #endif
        [NinoMember(1)][PropertyOrder(int.MinValue)]
        public int localId;
        [NinoMember(2)]
        public Vector3 position;

        public abstract Zone CreateZone(Gear gear);
    }
}