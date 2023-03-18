using System.Collections.Generic;
using LitJson.Extensions;
using Nino.Serialization;
using UnityEngine;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigSceneGroup
    {
        [NinoMember(1)]
        [PropertyOrder(int.MinValue)]
        public ulong id;
#if UNITY_EDITOR
        [SerializeField] [LabelText("策划备注")] [PropertyOrder(int.MinValue + 1)]
        private string remarks;
#endif
        [NinoMember(2)]
        [Tooltip("实体")]
        public ConfigSceneGroupActor[] actors;
        [NinoMember(3)]
        [Tooltip("触发区域")]
        public ConfigSceneGroupZone[] zones;
        [NinoMember(4)]
        [Tooltip("事件监听")]
        public ConfigSceneGroupTrigger[] triggers;
        [NinoMember(5)]
        [Tooltip("组")]
        public ConfigSceneGroupSuites[] suites;
        [NinoMember(6)]
        [Tooltip("寻路路径")]
        public ConfigRoute[] route;
        [NinoMember(7)]
        [LabelText("是否初始随机一个组？")]
        public bool randSuite;
        [NinoMember(8)]
        [LabelText("初始组")]
        [ShowIf("@!randSuite")] [ValueDropdown("@OdinDropdownHelper.GetSceneGroupSuiteIds()")]
        public int initSuite;
    }
}