using DaGenGraph;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    public class SceneGroupGraph: JsonGraphBase
    {
        public ulong Id;
        [LabelText("策划备注")]
        public string Remarks;
        public Vector3 Position;
        public Vector3 Rotation;
        [Tooltip("实体")]
        public ConfigSceneGroupActor[] Actors;
        [Tooltip("触发区域")]
        public ConfigSceneGroupZone[] Zones;
        [LabelText("是否初始随机一个组？")]
        public bool RandSuite;

        [LabelText("初始组")]
        [ShowIf("@!"+nameof(RandSuite))]
        public int InitSuite;
    }
}