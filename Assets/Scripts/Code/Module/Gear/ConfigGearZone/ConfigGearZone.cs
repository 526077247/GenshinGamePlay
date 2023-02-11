using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 区域
    /// </summary>
    [Serializable]
    public abstract class ConfigGearZone
    {
        
        #if UNITY_EDITOR
        [LabelText("策划备注")][SerializeField][PropertyOrder(int.MinValue+1)]
        private string remarks;
        #endif
        [PropertyOrder(int.MinValue)]
        [SerializeField] public int localId;
        [SerializeField] public Vector3 position;

        public abstract Zone CreateZone(Gear gear);
    }
}