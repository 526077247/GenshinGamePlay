#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEditor;

namespace TaoTie
{
    public static class OdinDropdownHelper
    {
        
        #region Gear
        public static ConfigGear gear;
        public static IEnumerable<Type> GetFilteredConditionTypeList(Type type)
        {
            if (type == null)
            {
                var q1 = typeof(ConfigGearTrigger).Assembly.GetTypes()
                    .Where(x => !x.IsAbstract)
                    .Where(x => !x.IsGenericTypeDefinition)
                    .Where(x => typeof(ConfigGearCondition).IsAssignableFrom(x));
                return q1;
            }
            var q2 = typeof(ConfigGearTrigger).Assembly.GetTypes()
                .Where(x => !x.IsAbstract)
                .Where(x => !x.IsGenericTypeDefinition)
                .Where(x =>
                {
                    bool has = false;
                    foreach (var attributeData in x.GetAttributes())
                    {
                        if (attributeData is TriggerTypeAttribute attr)
                        {
                            has = true;
                            if (attr.type == type || attr.type == null)
                            {
                                return true;
                            }
                        }
                    }
                    return !has;
                })
                .Where(x => typeof(ConfigGearCondition).IsAssignableFrom(x));
            return q2;
        }
        
        public static IEnumerable<Type> GetFilteredActionTypeList(Type type)
        {
            if (type == null)
            {
                var q1 = typeof(ConfigGearTrigger).Assembly.GetTypes()
                    .Where(x => !x.IsAbstract)
                    .Where(x => !x.IsGenericTypeDefinition)
                    .Where(x => typeof(ConfigGearAction).IsAssignableFrom(x));
                return q1;
            }
            var q2 = typeof(ConfigGearTrigger).Assembly.GetTypes()
                .Where(x => !x.IsAbstract)
                .Where(x => !x.IsGenericTypeDefinition)
                .Where(x =>
                {
                    bool has = false;
                    foreach (var attributeData in x.GetAttributes())
                    {
                        if (attributeData is TriggerTypeAttribute attr)
                        {
                            has = true;
                            if (attr.type == type || attr.type == null)
                            {
                                return true;
                            }
                        }
                    }
                    return !has;
                })
                .Where(x => typeof(ConfigGearAction).IsAssignableFrom(x));
            return q2;
        }

        public static List<int> GetGearActorIds()
        {
            
            List<int> res = new List<int>();
            if (gear!=null)
            {
                ConfigGear config = gear;
                for (int i = 0; i < (config.actors == null ? 0 : config.actors.Length); i++)
                {
                    if (config.actors[i] != null)
                        res.Add(config.actors[i].localId);
                }
            }
            
            return res;
        }

        public static List<int> GetGearRouteIds()
        {
            List<int> res = new List<int>();
            if (gear!=null)
            {
                ConfigGear config = gear;
                for (int i = 0; i < (config.route == null ? 0 : config.route.Length); i++)
                {
                    if (config.route[i] != null)
                        res.Add(config.route[i].localId);
                }
            }
            return res;
        }

        public static List<int> GetGearZoneIds()
        {
            List<int> res = new List<int>();
            if (gear!=null)
            {
                ConfigGear config = gear;
                for (int i = 0; i < (config.zones == null ? 0 : config.zones.Length); i++)
                {
                    if (config.zones[i] != null)
                        res.Add(config.zones[i].localId);
                }
            }
            return res;
        }

        public static List<int> GetGearGroupIds()
        {
            List<int> res = new List<int>();
            if (gear!=null)
            {
                ConfigGear config = gear;
                for (int i = 0; i < (config.group == null ? 0 : config.group.Length); i++)
                {
                    if (config.group[i] != null)
                        res.Add(config.group[i].localId);
                }
            }
            return res;
        }
        public static List<int> GetGearGroupIds0()
        {
            List<int> res = new List<int>();
            if (gear!=null)
            {
                ConfigGear config = gear;
                for (int i = 0; i < (config.group == null ? 0 : config.group.Length); i++)
                {
                    if (config.group[i] != null)
                        res.Add(config.group[i].localId);
                }
            }
            res.Add(0);
            return res;
        }
        public static List<int> GetGearTriggerIds()
        {
            List<int> res = new List<int>();
            if (gear!=null)
            {
                ConfigGear config = gear;
                for (int i = 0; i < (config.triggers == null ? 0 : config.triggers.Length); i++)
                {
                    if (config.triggers[i] != null)
                        res.Add(config.triggers[i].localId);
                }
            }
            return res;
        }
        #endregion

        #region Ability
        /// <summary>
        /// 数值类型
        /// </summary>
        /// <returns></returns>
        public static IEnumerable GetNumericTypeId()
        {
            var fields = typeof(NumericType).GetFields();
            ValueDropdownList<int> list = new ValueDropdownList<int>();
            if (fields.Length > 0)
            {
                for (int i = 0; i < fields.Length; i++)
                {
                    if (!fields[i].IsStatic)
                    {
                        continue;
                    }
                    var val = (int) fields[i].GetValue(null);
                    if (val <= NumericType.Max) continue;
                    list.Add($"{fields[i].Name}({val})", val);
                }
            }
            return list;
        }
        /// <summary>
        /// 阵营类型
        /// </summary>
        /// <returns></returns>
        public static IEnumerable GetCampTypeId()
        {
            var fields = typeof(CampConst).GetFields();
            ValueDropdownList<uint> list = new ValueDropdownList<uint>();
            if (fields.Length > 0)
            {
                for (int i = 0; i < fields.Length; i++)
                {
                    if (!fields[i].IsStatic)
                    {
                        continue;
                    }
                    var val = (uint) fields[i].GetValue(null);
                    list.Add($"{fields[i].Name}({val})", val);
                }
               
            }
            return list;
        }
        #endregion
    }
}
#endif