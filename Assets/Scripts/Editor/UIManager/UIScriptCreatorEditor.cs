using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace TaoTie
{
    public class UIScriptCreatorEditor : Editor
    {
        static string GetPrefabPath()
        {
            var prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage == null)
            {
                Debug.LogError("当前不是prefab编辑窗口，请打开prefab编辑窗口进行操作");
                return "";
            }

            string prefabPath = prefabStage.prefabAssetPath;
            string addressable_path = "Assets/AssetsPackage/";
            if (prefabPath.Contains(addressable_path))
            {
                prefabPath = prefabPath.Replace(addressable_path, "");
            }

            return prefabPath;
        }

        [MenuItem("GameObject/生成UI代码/根据选择节点生成UI代码", false, 23)]
        static void CreateUIModule()
        {
            var selected = Selection.gameObjects;
            if (selected == null || selected.Length == 0)
            {
                Debug.LogError("未选中节点");
                return;
            }

            string prefabPath = GetPrefabPath();
            UIScriptController.GenerateUICode(selected, prefabPath);

            Debug.Log("生成完成");
        }

        [MenuItem("Assets/工具/UI/绑定节点",false,423)]
        [MenuItem("GameObject/生成UI代码/绑定节点", false, 25)]
        static void Generate()
        {
            string prefabPath;
            var obj = Selection.activeObject as GameObject;
            if (obj == null)
            {
                return;
            }
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (!string.IsNullOrEmpty(path))
            {
                prefabPath = path;
            }
            else
            {
                var prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
                if (prefabStage != null)
                {
                    prefabPath = prefabStage.assetPath;
                }
                else
                {
                    return;
                }
            }
            
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            var rcPrefab = go.GetComponent<ReferenceCollector>();
            if (rcPrefab == null)
            {
                rcPrefab = go.AddComponent<ReferenceCollector>();
            }
            else
            {
                rcPrefab.Clear();
            }
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
                var viewType = typeof(UIBaseContainer);
                var type = assembly.GetType("TaoTie." + go.name);
                BindingFlags flag = BindingFlags.Static | BindingFlags.Public;

                if (type != viewType && viewType.IsAssignableFrom(type))
                {
                    UIBaseContainer ui = Activator.CreateInstance(type) as UIBaseContainer;
                    if (ui is IOnCreate onCreate)
                    {
                        ui.SetTransform(go.transform);
                        try
                        {
                            onCreate.OnCreate();
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError(ex);
                        }
                    }
                }
                
                EditorUtility.SetDirty(go);
            }

            AssetDatabase.SaveAssetIfDirty(go);
            AssetDatabase.Refresh();

            Debug.Log("生成完成");
        }
    }
}