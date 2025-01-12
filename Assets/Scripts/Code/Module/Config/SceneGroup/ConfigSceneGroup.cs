using System.Collections.Generic;
using LitJson.Extensions;
using Nino.Core;
using UnityEngine;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigSceneGroup
    {
        [NinoMember(1)]
        [PropertyOrder(int.MinValue)]
        public ulong Id;
#if UNITY_EDITOR
        [LabelText("策划备注")] [PropertyOrder(int.MinValue + 1)]
        public string Remarks;
#endif
        [NinoMember(2)]
        public Vector3 Position;
        [NinoMember(3)]
        public Vector3 Rotation;
        [NinoMember(4)]
        [Tooltip("实体")]
        public ConfigSceneGroupActor[] Actors;
        [NinoMember(5)]
        [Tooltip("触发区域")]
        public ConfigSceneGroupZone[] Zones;
        [NinoMember(6)]
        [Tooltip("事件监听")]
        public ConfigSceneGroupTrigger[] Triggers;
        [NinoMember(7)]
        [Tooltip("组")]
        public ConfigSceneGroupSuites[] Suites;
        [NinoMember(8)]
        [Tooltip("寻路路径")]
        public ConfigRoute[] Route;
        [NinoMember(9)]
        [LabelText("是否初始随机一个组？")]
        public bool RandSuite;
        [NinoMember(10)]
        [LabelText("初始组")]
        [ShowIf("@!"+nameof(RandSuite))]
#if UNITY_EDITOR
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetSceneGroupSuiteIds)+"()",AppendNextDrawer = true)]
#endif
        public int InitSuite;
    }
}