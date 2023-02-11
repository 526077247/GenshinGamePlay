using System;
using Sirenix.OdinInspector;
using UnityEngine;


namespace TaoTie
{
    public abstract class ConfigGearAction
    {
        [LabelText("禁用")] public bool disable;

#if UNITY_EDITOR
        public Type handleType;
#endif
        [LabelText("排序序号")] public int localId;
        public virtual bool canSetOtherGear { get; } = false;

        [SerializeField] [ShowIf("canSetOtherGear")] [LabelText("是否是设置其他Gear的内容")] 
        public bool isOtherGear;

        [SerializeField] [ShowIf("isOtherGear")]
        public ulong otherGearId;
        public void ExecuteAction(IEventBase evt, Gear gear)
        {
            if (disable)
            {
                // NLog.Info(LogConst.NGear, "被禁用");
                return;
            }

            var aimGear = gear;
            if (isOtherGear)
            {
                if (gear.manager.TryGetGear(otherGearId, out var other))
                {
                    aimGear = other;
                }
                else
                {
                    Log.Error("未找到其他Gear,请检查配置! id=" + otherGearId);
                    return;
                }
            }

            Execute(evt, aimGear, gear);
        }

        protected abstract void Execute(IEventBase evt, Gear aimGear, Gear fromGear);
    }
}