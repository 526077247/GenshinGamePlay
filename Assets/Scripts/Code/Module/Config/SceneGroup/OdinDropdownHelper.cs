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
        
        #region SceneGroup
        public static ConfigSceneGroup sceneGroup;
        public static IEnumerable<Type> GetFilteredConditionTypeList(Type type)
        {
            if (type == null)
            {
                var q1 = typeof(ConfigSceneGroupTrigger).Assembly.GetTypes()
                    .Where(x => !x.IsAbstract)
                    .Where(x => !x.IsGenericTypeDefinition)
                    .Where(x => typeof(ConfigSceneGroupCondition).IsAssignableFrom(x));
                return q1;
            }
            var q2 = typeof(ConfigSceneGroupTrigger).Assembly.GetTypes()
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
                .Where(x => typeof(ConfigSceneGroupCondition).IsAssignableFrom(x));
            return q2;
        }
        
        public static IEnumerable<Type> GetFilteredActionTypeList(Type type)
        {
            if (type == null)
            {
                var q1 = typeof(ConfigSceneGroupTrigger).Assembly.GetTypes()
                    .Where(x => !x.IsAbstract)
                    .Where(x => !x.IsGenericTypeDefinition)
                    .Where(x => typeof(ConfigSceneGroupAction).IsAssignableFrom(x));
                return q1;
            }
            var q2 = typeof(ConfigSceneGroupTrigger).Assembly.GetTypes()
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
                .Where(x => typeof(ConfigSceneGroupAction).IsAssignableFrom(x));
            return q2;
        }

        public static List<int> GetSceneGroupActorIds()
        {
            
            List<int> res = new List<int>();
            if (sceneGroup!=null)
            {
                ConfigSceneGroup config = sceneGroup;
                for (int i = 0; i < (config.actors == null ? 0 : config.actors.Length); i++)
                {
                    if (config.actors[i] != null)
                        res.Add(config.actors[i].localId);
                }
            }
            
            return res;
        }

        public static List<int> GetSceneGroupRouteIds()
        {
            List<int> res = new List<int>();
            if (sceneGroup!=null)
            {
                ConfigSceneGroup config = sceneGroup;
                for (int i = 0; i < (config.route == null ? 0 : config.route.Length); i++)
                {
                    if (config.route[i] != null)
                        res.Add(config.route[i].localId);
                }
            }
            return res;
        }

        public static List<int> GetSceneGroupZoneIds()
        {
            List<int> res = new List<int>();
            if (sceneGroup!=null)
            {
                ConfigSceneGroup config = sceneGroup;
                for (int i = 0; i < (config.zones == null ? 0 : config.zones.Length); i++)
                {
                    if (config.zones[i] != null)
                        res.Add(config.zones[i].localId);
                }
            }
            return res;
        }

        public static List<int> GetSceneGroupSuiteIds()
        {
            List<int> res = new List<int>();
            if (sceneGroup!=null)
            {
                ConfigSceneGroup config = sceneGroup;
                for (int i = 0; i < (config.suites == null ? 0 : config.suites.Length); i++)
                {
                    if (config.suites[i] != null)
                        res.Add(config.suites[i].localId);
                }
            }
            return res;
        }
        public static List<int> GetSceneGroupSuiteIds0()
        {
            List<int> res = new List<int>();
            if (sceneGroup!=null)
            {
                ConfigSceneGroup config = sceneGroup;
                for (int i = 0; i < (config.suites == null ? 0 : config.suites.Length); i++)
                {
                    if (config.suites[i] != null)
                        res.Add(config.suites[i].localId);
                }
            }
            res.Add(0);
            return res;
        }
        public static List<int> GetSceneGroupTriggerIds()
        {
            List<int> res = new List<int>();
            if (sceneGroup!=null)
            {
                ConfigSceneGroup config = sceneGroup;
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

        #region AI

        /// <summary>
        /// 过滤类型
        /// </summary>
        /// <returns></returns>
        public static IEnumerable GetAIDecisionInterface()
        {
            var methods = typeof(AIDecisionInterface).GetMethods();
            ValueDropdownList<string> list = new ValueDropdownList<string>();
            if (methods.Length > 0)
            {
                for (int i = 0; i < methods.Length; i++)
                {
                    if (!methods[i].IsStatic)
                    {
                        continue;
                    }

                    var attrs = methods[i].GetCustomAttributes(TypeInfo<LabelTextAttribute>.Type,false);
                    string val;
                    if (attrs.Length > 0)
                    {
                        val = $"{(attrs[0] as LabelTextAttribute).Text}({methods[i].Name})";
                    }
                    else
                    {
                        val = methods[i].Name;
                    }
                    list.Add(val, val);
                }
               
            }
            return list;
        }

        #endregion
    }
}
#endif