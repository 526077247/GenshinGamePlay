using System.Collections.Generic;
using LitJson.Extensions;
using Nino.Serialization;
using UnityEngine;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigGear
    {
        [NinoMember(1)]
        [PropertyOrder(int.MinValue)]
        public ulong id;
#if UNITY_EDITOR
        [LabelText("策划备注")] [PropertyOrder(int.MinValue + 1)]
        private string remarks;
#endif
        [NinoMember(2)]
        [Tooltip("实体")]
        public ConfigGearActor[] actors;
        [NinoMember(3)]
        [Tooltip("触发区域")]
        public ConfigGearZone[] zones;
        [NinoMember(4)]
        [Tooltip("事件监听")]
        public ConfigGearTrigger[] triggers;
        [NinoMember(5)]
        [Tooltip("组")]
        public ConfigGearGroup[] group;
        [NinoMember(6)]
        [Tooltip("寻路路径")]
        public ConfigRoute[] route;
        [NinoMember(7)]
        [LabelText("是否初始随机一个组？")]
        public bool randGroup;
        [NinoMember(8)]
        [LabelText("初始组")]
        [ShowIf("@!randGroup")] [ValueDropdown("@OdinDropdownHelper.GetGearGroupIds()")]
        public int initGroup;
    }
}