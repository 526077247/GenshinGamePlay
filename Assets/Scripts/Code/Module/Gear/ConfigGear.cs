using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

namespace TaoTie
{
    public class ConfigGear : SerializedScriptableObject
    {
        [PropertyOrder(int.MinValue)] [SerializeField]
        public ulong id;
#if UNITY_EDITOR
        [LabelText("策划备注")] [SerializeField] [PropertyOrder(int.MinValue + 1)]
        private string remarks;
#endif
        [Tooltip("实体")]
        [SerializeReference] public ConfigGearActor[] actors;
        [Tooltip("触发区域")]
        [SerializeReference] public ConfigGearZone[] zones;
        [Tooltip("事件监听")]
        [SerializeReference] public ConfigGearTrigger[] triggers;
        [Tooltip("组")]
        [OnCollectionChanged("Refresh")] [OnStateUpdate("Refresh")]
        public ConfigGearGroup[] group;
        [Tooltip("寻路路径")]
        [OnCollectionChanged("RefreshActor")] [OnStateUpdate("RefreshActor")] [SerializeReference]
        public ConfigRoute[] route;
        [LabelText("是否初始随机一个组？")]
        [SerializeField] public bool randGroup;
        [LabelText("初始组")]
        [ShowIf("@!randGroup")] [ValueDropdown("groupIds")] [SerializeField]
        public int initGroup;

        // [ValueDropdown("groupIds0")] [SerializeField]
        // public int endGroup;

#if UNITY_EDITOR
        
        private List<int> groupIds
        {
            get
            {
                List<int> res = new List<int>();
                if (group == null) return res;
                for (int i = 0; i < group.Length; i++)
                {
                    if (group[i] != null)
                        res.Add(group[i].localId);
                }

                return res;
            }
        }
        
        private List<int> groupIds0
        {
            get
            {
                List<int> res = new List<int>();
                if (group == null) return res;
                for (int i = 0; i < group.Length; i++)
                {
                    if (group[i] != null)
                        res.Add(group[i].localId);
                }

                res.Add(0); //需要额外加个0
                return res;
            }
        }

        
        private void Refresh()
        {
            if (group != null)
            {
                for (int i = 0; i < group.Length; i++)
                {
                    if (group[i] != null)
                    {
                        group[i].gearActors = this.actors;
                        group[i].gearZones = this.zones;
                        group[i].gearTriggers = this.triggers;
                        group[i].randGroup = this.randGroup;
                    }
                }
            }
        }

        
        private void RefreshActor()
        {
            if (actors != null)
            {
                for (int i = 0; i < actors.Length; i++)
                {
                    if (actors[i] != null)
                    {
                        actors[i].routes = this.route;
                    }
                }
            }
        }

#endif
    }
}