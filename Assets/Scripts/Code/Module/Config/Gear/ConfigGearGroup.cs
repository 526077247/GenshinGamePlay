using System;
using System.Collections.Generic;
using System.Linq;
using LitJson.Extensions;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 小组配置
    /// </summary>
    [NinoSerialize]
    public partial class ConfigGearGroup
    {
#if UNITY_EDITOR
        [LabelText("策划备注")][PropertyOrder(int.MinValue+1)]
        private string remarks;
        [JsonIgnore]
        private bool randGroup => OdinDropdownHelper.gear.randGroup;
#endif
        [NinoMember(1)][PropertyOrder(int.MinValue)]
        public int localId;
        [NinoMember(2)][ValueDropdown("@OdinDropdownHelper.GetGearActorIds()")]
        public int[] actors;
        [NinoMember(3)][ValueDropdown("@OdinDropdownHelper.GetGearZoneIds()")]
        public int[] zones;
        [NinoMember(4)][ValueDropdown("@OdinDropdownHelper.GetGearTriggerIds()")]
        public int[] triggers;
        [NinoMember(5)][ShowIf(nameof(randGroup))]
        public int randWeight;
        
        
    }
}