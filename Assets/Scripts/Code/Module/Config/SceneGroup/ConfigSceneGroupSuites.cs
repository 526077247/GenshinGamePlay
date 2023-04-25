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
    public partial class ConfigSceneGroupSuites
    {
#if UNITY_EDITOR
        [SerializeField] [LabelText("策划备注")][PropertyOrder(int.MinValue+1)]
        private string Remarks;
        [JsonIgnore]
        private bool RandSuite => OdinDropdownHelper.sceneGroup.RandSuite;
#endif
        [NinoMember(1)][PropertyOrder(int.MinValue)]
        public int LocalId;
        [NinoMember(2)][ValueDropdown("@OdinDropdownHelper.GetSceneGroupActorIds()")]
        public int[] Actors;
        [NinoMember(3)][ValueDropdown("@OdinDropdownHelper.GetSceneGroupZoneIds()")]
        public int[] Zones;
        [NinoMember(4)][ValueDropdown("@OdinDropdownHelper.GetSceneGroupTriggerIds()")]
        public int[] Triggers;
#if UNITY_EDITOR
        [ShowIf(nameof(RandSuite))]
#endif
        [NinoMember(5)]
        public int RandWeight;
        
        
    }
}