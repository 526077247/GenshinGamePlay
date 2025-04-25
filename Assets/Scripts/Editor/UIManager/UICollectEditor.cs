using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace TaoTie
{
    public class UICollectEditor
    {
        [MenuItem("Tools/美术工具/绑定节点", false, 23)]
        public static void Generate()
        {
            Assembly assembly = null;
            foreach (var item in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (item.FullName.Contains("Unity.Code"))
                {
                    assembly = item;
                    Debug.Log("Get AOT Dll Success");
                    break;
                }
            }

            if (assembly != null)
            {
                GameObject prefab = null;
                var viewType = typeof(UIBaseView);
                var types = assembly.GetTypes();
                BindingFlags flag = BindingFlags.Static|BindingFlags.Public;
                for (int i = 0; i < types.Length; i++)
                {
                    if (types[i]!= viewType && viewType.IsAssignableFrom(types[i]))
                    {
                        prefab = null;
                        var props = types[i].GetProperties(flag);
                        for (int j = 0; j < props.Length; j++)
                        {
                            var str = props[j].GetValue(null);
                            prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AssetsPackage/" + str);
                            if (prefab != null)
                            {
                                break;
                            }
                        }
                        if (prefab == null)
                        {
                            var fields = types[i].GetFields(flag);
                            for (int j = 0; j < fields.Length; j++)
                            {
                                var str = fields[j].GetValue(null);
                                prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AssetsPackage/" + str);
                                if (prefab != null)
                                {
                                    break;
                                }
                            }
                        }

                        if (prefab != null)
                        {
                            var obj = GameObject.Instantiate(prefab);
                            var rc = obj.GetComponent<ReferenceCollector>();
                            if (rc == null)
                            {
                                rc = obj.AddComponent<ReferenceCollector>();
                            }
                            else
                            {
                                rc.Clear();
                            }
                            UIBaseView ui = Activator.CreateInstance(types[i]) as UIBaseView;
                            if (ui is IOnCreate onCreate)
                            {
                                ui.SetTransform(obj.transform);
                                try
                                {
                                    onCreate.OnCreate();
                                }
                                catch (Exception ex)
                                {
                                    Debug.LogError(ex);
                                }
                            }
                            var rcPrefab = prefab.GetComponent<ReferenceCollector>();
                            if (rcPrefab == null)
                            {
                                rcPrefab = prefab.AddComponent<ReferenceCollector>();
                            }
                            else
                            {
                                rcPrefab.Clear();
                            }

                            foreach (var data in rc.data)
                            {
                                var trans = rcPrefab.transform.Find(data.key);
                                if (trans != null) rcPrefab.Add(data.key, trans);
                            }
                            AssetDatabase.SaveAssetIfDirty(prefab);
                            GameObject.DestroyImmediate(obj);
                        }
                    }
                }
                AssetDatabase.Refresh();
            }
        }
    }
}