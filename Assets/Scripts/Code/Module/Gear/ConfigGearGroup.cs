using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 小组配置
    /// </summary>
    [Serializable]
    public class ConfigGearGroup
    {
#if UNITY_EDITOR
        [LabelText("策划备注")][SerializeField][PropertyOrder(int.MinValue+1)]
        private string remarks;
        /// <summary>
        /// 编辑器下使用！！加个Obsolete给提示
        /// </summary>
        [HideInInspector]
        [Obsolete][HideInTables][NonSerialized]
        public ConfigGearActor[] gearActors;
        [HideInInspector]
        [Obsolete][HideInTables][NonSerialized]
        public ConfigGearZone[] gearZones;
        [HideInInspector]
        [Obsolete][HideInTables][NonSerialized]
        public ConfigGearTrigger[] gearTriggers;
        [HideInInspector]
        [Obsolete][HideInTables][NonSerialized]
        public bool randGroup;
        
        private List<int> actorIds
        {
            get
            {
                List<int> res = new List<int>();
                if (gearActors == null) return res;
                for (int i = 0; i < gearActors.Length; i++)
                {
                    if(gearActors[i]!=null&&(actors==null||!actors.Contains(gearActors[i].localId)))
                        res.Add(gearActors[i].localId);
                }

                return res;
            }
        }
        
        private List<int> zoneIds
        {
            get
            {
                List<int> res = new List<int>();
                if (gearZones == null) return res;
                for (int i = 0; i <gearZones.Length; i++)
                {
                    if(gearZones[i]!=null&&(zones==null||!zones.Contains(gearZones[i].localId)))
                        res.Add(gearZones[i].localId);
                }

                return res;
            }
        }
        
        private List<int> triggerIds
        {
            get
            {
                List<int> res = new List<int>();
                if (gearTriggers == null) return res;
                for (int i = 0; i < gearTriggers.Length; i++)
                {
                    if(gearTriggers[i]!=null&&(triggers==null||!triggers.Contains(gearTriggers[i].localId)))
                        res.Add(gearTriggers[i].localId);
                }

                return res;
            }
        }
#endif
        [PropertyOrder(int.MinValue)]
        [SerializeField]
        public int localId;
        [ValueDropdown("actorIds")][SerializeField]
        public int[] actors;
        [ValueDropdown("zoneIds")][SerializeField]
        public int[] zones;
        [ValueDropdown("triggerIds")][SerializeField]
        public int[] triggers;
        [ShowIf("randGroup")][SerializeField]
        public int randWeight;
        
        
    }
}