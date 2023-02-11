using System;
using Sirenix.OdinInspector;
using UnityEngine;


namespace TaoTie
{
    [Serializable]
    public abstract class ConfigGearAction
    {
        [SerializeField] [LabelText("禁用")] public bool disable;

#if UNITY_EDITOR
        [HideInInspector] public Type handleType;
#endif
        [SerializeField] [LabelText("排序序号")] public int localId;
        public virtual bool canSetOtherGear { get; } = false;

        [SerializeField] [ShowIf("canSetOtherGear")] [LabelText("是否是设置其他Gear的内容")] 
        public bool isOtherGear;

        [SerializeField] [ShowIf("isOtherGear")]
        public ulong otherGearId;
    }
}