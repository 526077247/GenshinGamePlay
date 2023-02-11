using System;
using Sirenix.OdinInspector;
using UnityEngine;

#pragma warning disable 0649

namespace TaoTie
{
    [Serializable]
    public abstract class ConfigGearActor
    {
#if UNITY_EDITOR
        /// <summary>
        /// 编辑器下使用！！加个Obsolete给提示
        /// </summary>
        [HideInInspector]
        [Obsolete][HideInTables][NonSerialized]
        public ConfigRoute[] routes;
        [PropertyOrder(int.MinValue+1)]
        [LabelText("策划备注")][SerializeField]
        private string remarks;
#endif
        [PropertyOrder(int.MinValue)]
        [SerializeField] public int localId;
        [SerializeField] public Vector3 position;
        
        public abstract Entity CreateActor(Gear gear);
    }
}