#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

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
                            if (attr.Type == type || attr.Type == null)
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
                            if (attr.Type == type || attr.Type == null)
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
                for (int i = 0; i < (config.Actors == null ? 0 : config.Actors.Length); i++)
                {
                    if (config.Actors[i] != null)
                        res.Add(config.Actors[i].LocalId);
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
                for (int i = 0; i < (config.Route == null ? 0 : config.Route.Length); i++)
                {
                    if (config.Route[i] != null)
                        res.Add(config.Route[i].LocalId);
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
                for (int i = 0; i < (config.Zones == null ? 0 : config.Zones.Length); i++)
                {
                    if (config.Zones[i] != null)
                        res.Add(config.Zones[i].LocalId);
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
                for (int i = 0; i < (config.Suites == null ? 0 : config.Suites.Length); i++)
                {
                    if (config.Suites[i] != null)
                        res.Add(config.Suites[i].LocalId);
                }
            }
            return res;
        }
        public static IEnumerable GetMonsterConfigIds()
        {
            var textAssets = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/AssetsPackage/Config/MonsterConfigCategory.bytes");
            var textAssets2 = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/AssetsPackage/Config/UnitConfigCategory.bytes");
            ValueDropdownList<int> list = new ValueDropdownList<int>();
            if(textAssets!=null&&textAssets2!=null)
            {
                MonsterConfigCategory monster = ProtobufHelper.FromBytes<MonsterConfigCategory>(textAssets.bytes);
                UnitConfigCategory unit = ProtobufHelper.FromBytes<UnitConfigCategory>(textAssets2.bytes);
                for (int i = 0; i < monster.GetAllList().Count; i++)
                {
                    var item = monster.GetAllList()[i];
                    var u = unit.Get(item.UnitId);
                    string name = u == null ? ("未找到UnitId" + item.UnitId) : u.Name;
                    list.Add($"{name}({item.Id})", item.Id);
                }
            }
            return list;
        }
        
        public static IEnumerable GetGadgetConfigIds()
        {
            var textAssets = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/AssetsPackage/Config/GadgetConfigCategory.bytes");
            var textAssets2 = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/AssetsPackage/Config/UnitConfigCategory.bytes");
            ValueDropdownList<int> list = new ValueDropdownList<int>();
            if(textAssets!=null&&textAssets2!=null)
            {
                GadgetConfigCategory gadget = ProtobufHelper.FromBytes<GadgetConfigCategory>(textAssets.bytes);
                UnitConfigCategory unit = ProtobufHelper.FromBytes<UnitConfigCategory>(textAssets2.bytes);
                for (int i = 0; i < gadget.GetAllList().Count; i++)
                {
                    var item = gadget.GetAllList()[i];
                    var u = unit.Get(item.UnitId);
                    string name = u == null ? ("未找到UnitId" + item.UnitId) : u.Name;
                    list.Add($"{name}({item.Id})", item.Id);
                }
            }
            return list;
        }
        
        public static IEnumerable GetCharacterConfigIds()
        {
            var textAssets = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/AssetsPackage/Config/CharacterConfigCategory.bytes");
            ValueDropdownList<int> list = new ValueDropdownList<int>();
            if(textAssets!=null)
            {
                CharacterConfigCategory character = ProtobufHelper.FromBytes<CharacterConfigCategory>(textAssets.bytes);
                for (int i = 0; i < character.GetAllList().Count; i++)
                {
                    var item = character.GetAllList()[i];
                    list.Add($"{item.Name}({item.Id})", item.Id);
                }
            }
            return list;
        }
        public static List<int> GetSceneGroupTriggerIds()
        {
            List<int> res = new List<int>();
            if (sceneGroup!=null)
            {
                ConfigSceneGroup config = sceneGroup;
                for (int i = 0; i < (config.Triggers == null ? 0 : config.Triggers.Length); i++)
                {
                    if (config.Triggers[i] != null)
                        res.Add(config.Triggers[i].LocalId);
                }
            }
            return res;
        }
        
        public static List<int> GetEnvironmentConfigIds()
        {
            List<int> res = new List<int>();
            var textAssets = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/AssetsPackage/EditConfig/ConfigEnvironments.json");
            if(textAssets!=null)
            {
                ConfigEnvironments envs = ProtobufHelper.FromBytes<ConfigEnvironments>(textAssets.bytes);
                if(envs?.DefaultEnvironment!=null)
                    res.Add(envs.DefaultEnvironment.Id);
                if (envs?.Environments != null)
                {
                    for (int i = 0; i < envs.Environments.Length; i++)
                    {
                        res.Add(envs.Environments[i].Id);
                    }
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
        /// <summary>
        /// GadgetState
        /// </summary>
        /// <returns></returns>
        public static IEnumerable GetGadgetState()
        {
            var fields = typeof(GadgetState).GetFields();
            ValueDropdownList<int> list = new ValueDropdownList<int>();
            if (fields.Length > 0)
            {
                for (int i = 0; i < fields.Length; i++)
                {
                    if (!fields[i].IsStatic)
                    {
                        continue;
                    }
                    var val = (int)fields[i].GetValue(null);
                    list.Add($"{fields[i].Name}({val})", val);
                }
               
            }
            return list;
        }
        /// <summary>
        /// 获取所有abilityName
        /// </summary>
        /// <returns></returns>
        public static IEnumerable GetAbilities()
        {
            var files = Directory.GetFiles("Assets/AssetsPackage/EditConfig/Abilities");
            ValueDropdownList<string> list = new ValueDropdownList<string>();
            HashSet<string> temp = new HashSet<string>();
            for (int i = 0; i < files.Length; i++)
            {
                var jStr = File.ReadAllText(files[i]);
                if (JsonHelper.TryFromJson(jStr, out List<ConfigAbility> abilities))
                {
                    for (int j = 0; j < abilities.Count; j++)
                    {
                        if (temp.Contains(abilities[j].AbilityName))
                        {
                            Log.Error("ability name重复！"+abilities[j].AbilityName+"  路径："+files[i]);
                            continue;
                        }

                        temp.Add(abilities[j].AbilityName);
                        list.Add(abilities[j].AbilityName);
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// 获取所有Effect
        /// </summary>
        /// <returns></returns>
        public static IEnumerable GetEffects()
        {
            var files = AssetDatabase.FindAssets("t:Prefab",new []{"Assets/AssetsPackage/Effect"});
            ValueDropdownList<string> list = new ValueDropdownList<string>();
            for (int i = 0; i < files.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(files[i]);

                list.Add(Path.GetFileNameWithoutExtension(path));
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