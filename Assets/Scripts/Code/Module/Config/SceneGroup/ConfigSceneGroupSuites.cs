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
        [LabelText("策划备注")][PropertyOrder(int.MinValue+1)]
        private string remarks;
        [JsonIgnore]
        private bool RandSuite => OdinDropdownHelper.sceneGroup.randSuite;
#endif
        [NinoMember(1)][PropertyOrder(int.MinValue)]
        public int localId;
        [NinoMember(2)][ValueDropdown("@OdinDropdownHelper.GetSceneGroupActorIds()")]
        public int[] actors;
        [NinoMember(3)][ValueDropdown("@OdinDropdownHelper.GetSceneGroupZoneIds()")]
        public int[] zones;
        [NinoMember(4)][ValueDropdown("@OdinDropdownHelper.GetSceneGroupTriggerIds()")]
        public int[] triggers;
        [NinoMember(5)][ShowIf(nameof(RandSuite))]
        public int randWeight;
        
        
    }
}