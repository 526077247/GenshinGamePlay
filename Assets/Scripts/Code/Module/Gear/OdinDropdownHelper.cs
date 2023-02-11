using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using UnityEditor;

namespace TaoTie
{
    public static class OdinDropdownHelper
    {
        #region Gear

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
                    foreach (var attributeData in x.GetAttributes())
                    {
                        if (attributeData is TriggerTypeAttribute attr)
                        {
                            if (attr.type == type || attr.type == null)
                            {
                                return true;
                            }
                        }
                    }
                    return false;
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
                    foreach (var attributeData in x.GetAttributes())
                    {
                        if (attributeData is TriggerTypeAttribute attr)
                        {
                            if (attr.type == type || attr.type == null)
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                })
                .Where(x => typeof(ConfigGearAction).IsAssignableFrom(x));
            return q2;
        }

        public static List<int> GetGearActorIds()
        {
            List<int> res = new List<int>();
            if (Selection.activeObject != null && Selection.activeObject.GetType() == typeof(ConfigGear))
            {
                ConfigGear config = Selection.activeObject as ConfigGear;
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
            if (Selection.activeObject != null && Selection.activeObject.GetType() == typeof(ConfigGear))
            {
                ConfigGear config = Selection.activeObject as ConfigGear;
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
            if (Selection.activeObject != null && Selection.activeObject.GetType() == typeof(ConfigGear))
            {
                ConfigGear config = Selection.activeObject as ConfigGear;
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
            if (Selection.activeObject != null && Selection.activeObject.GetType() == typeof(ConfigGear))
            {
                ConfigGear config = Selection.activeObject as ConfigGear;
                for (int i = 0; i < (config.group == null ? 0 : config.group.Length); i++)
                {
                    if (config.group[i] != null)
                        res.Add(config.group[i].localId);
                }
            }
            return res;
        }
        #endregion
    }
}