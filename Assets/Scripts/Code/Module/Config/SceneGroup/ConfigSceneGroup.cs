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
        [NinoMember(11)][LabelText("测试-是否关闭")]
        public bool Disable;
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
        [LabelText("*Actors,实体")][Tooltip("单个模板可支持使用\"通过ActorId创建实体\"Action创建实体的多个实例")]
        public ConfigSceneGroupActor[] Actors;
        [NinoMember(5)]
        [LabelText("Zones,触发区域")]
        public ConfigSceneGroupZone[] Zones;
        [NinoMember(6)]
        [LabelText("Triggers,事件监听")]
        public ConfigSceneGroupTrigger[] Triggers;
        [NinoMember(7)]
        [LabelText("Suites,阶段")]
        public ConfigSceneGroupSuites[] Suites;
        [NinoMember(8)]
        [LabelText("Route,寻路路径")]
        public ConfigRoute[] Route;
        [NinoMember(9)]
        [LabelText("是否初始随机一个阶段？")]
        public bool RandSuite;
        [NinoMember(10)]
        [LabelText("初始阶段")]
        [ShowIf("@!"+nameof(RandSuite))]
#if UNITY_EDITOR
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetSceneGroupSuiteIds)+"()",AppendNextDrawer = true)]
#endif
        public int InitSuite;
    }
}