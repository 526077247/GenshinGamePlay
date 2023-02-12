using System;
using LitJson.Extensions;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;


namespace TaoTie
{
    public abstract partial class ConfigGearAction
    {
        [NinoMember(1)]
        [LabelText("禁用")] public bool disable;

#if UNITY_EDITOR
        [HideInInspector][JsonIgnore]
        public Type handleType;
#endif
        [NinoMember(2)]
        [LabelText("排序序号")] public int localId;
        [JsonIgnore]
        public virtual bool canSetOtherGear { get; } = false;
        [ShowIf(nameof(canSetOtherGear))] [LabelText("是否是设置其他Gear的内容")] 
        [NinoMember(3)]
        public bool isOtherGear;

        [ShowIf(nameof(isOtherGear))]
        [NinoMember(4)]
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