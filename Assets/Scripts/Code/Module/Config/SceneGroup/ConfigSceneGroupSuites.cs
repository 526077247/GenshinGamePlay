using System;
using System.Collections.Generic;
using System.Linq;
using LitJson.Extensions;
using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 小组配置
    /// </summary>
    [NinoType(false)]
    public partial class ConfigSceneGroupSuites
    {
#if UNITY_EDITOR
        [LabelText("策划备注")][PropertyOrder(int.MinValue+1)]
        public string Remarks;
        [JsonIgnore]
        public bool RandSuite => OdinDropdownHelper.sceneGroup.RandSuite;
#endif
        [NinoMember(1)][PropertyOrder(int.MinValue)]
        public int LocalId;
        [NinoMember(2)]
#if UNITY_EDITOR
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetSceneGroupActorIds)+"()",AppendNextDrawer = true)]
#endif
        public int[] Actors;
        [NinoMember(3)]
#if UNITY_EDITOR
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetSceneGroupZoneIds)+"()",AppendNextDrawer = true)]
#endif
        public int[] Zones;
        [NinoMember(4)]
#if UNITY_EDITOR
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetSceneGroupTriggerIds)+"()",AppendNextDrawer = true)]
#endif
        public int[] Triggers;
#if UNITY_EDITOR
        [ShowIf(nameof(RandSuite))][LabelText("随机权值")]
#endif
        [NinoMember(5)]
        public int RandWeight;
        
        
    }
}