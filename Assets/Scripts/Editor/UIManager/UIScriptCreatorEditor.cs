using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace TaoTie
{
    public class UIScriptCreatorEditor : Editor
    {
        static bool IsMarking = false;
        static GameObject rootGo = null;


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

        [MenuItem("GameObject/生成UI代码/生成代码", false, 23)]
        static void CreateUIModule()
        {
            //GameObject go = menuCommand.context as GameObject;
            GameObject go = rootGo;

            if (go == null || go.GetComponent<UIScriptCreator>() == null)
            {
                Debug.LogError("未标记根节点");
                return;
            }

            string prefabPath = GetPrefabPath();
            UIScriptController.GenerateUICode(go, prefabPath);
            if (IsMarking)
            {
                IsMarking = false;
                EditorApplication.hierarchyWindowItemOnGUI -= DrawHierarchyIcon;
                return;
            }

            Debug.Log("生成完成");
        }

        [MenuItem("GameObject/生成UI代码/开始或取消标记", false, 22)]
        static void OpenMarkCreateUIFilesPanel()
        {
            if (IsMarking)
            {
                IsMarking = false;
                EditorApplication.hierarchyWindowItemOnGUI -= DrawHierarchyIcon;
                return;
            }

            IsMarking = true;
            EditorApplication.hierarchyWindowItemOnGUI += DrawHierarchyIcon;
        }

        [MenuItem("GameObject/生成UI代码/清除标记", false, 24)]
        static void ClearMark()
        {
            var prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage == null)
            {
                return;
            }

            string prefabPath = prefabStage.prefabAssetPath;
            var obj = Selection.activeObject as GameObject;
            if (obj == null)
            {
                return;
            }

            var trans = obj.transform;
            while (trans.parent != null)
            {
                trans = trans.parent;
            }

            var go = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            foreach (UIScriptCreator m in go.GetComponentsInChildren<UIScriptCreator>(true))
            {
                DestroyImmediate(m, true);
            }

            // 遍历标记生成代码的节点
            foreach (UIScriptCreator m in trans.GetComponentsInChildren<UIScriptCreator>(true))
            {
                DestroyImmediate(m, true);
            }

            EditorUtility.SetDirty(go);
            AssetDatabase.SaveAssetIfDirty(go);
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

        // 绘制icon方法
        static void DrawHierarchyIcon(int instanceID, Rect selectionRect)
        {
            // 设置icon的位置与尺寸（Hierarchy窗口的左上角是起点）
            float x = selectionRect.x + selectionRect.width - 120;
            float y = selectionRect.y;
            float h = selectionRect.height;

            GameObject go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (go == null)
            {
                return;
            }

            if (go.transform.parent == null)
            {
                GUI.Label(new Rect(x, y, 200, h), "生成变量名     是否生成");
                if (go.GetComponentsInChildren<Transform>().Length >= 2)
                {
                    rootGo = go.GetComponentsInChildren<Transform>()[1].gameObject;
                }

                return;
            }

            UIScriptCreator m;
            if (go.GetComponent<UIScriptCreator>() == null)
            {
                go.AddComponent<UIScriptCreator>();
                m = go.GetComponent<UIScriptCreator>();
                if (m == null) return;
                m.Mark(false);
                m.SetModuleName(go.name);
            }

            m = go.GetComponent<UIScriptCreator>();

            if (go == Selection.activeObject)
            {
                m.moduleName = GUI.TextArea(new Rect(x, y, 100, h), m.moduleName);
            }
            else
            {
                GUI.Label(new Rect(x, y, 100, h), m.moduleName);
            }

            if (go == rootGo)
            {
                m.Mark(true);
                GUI.Toggle(new Rect(x + 100, y, 20, h), true, "");
            }
            else
            {
                m.Mark(GUI.Toggle(new Rect(x + 100, y, 20, h), m.isMarked, ""));
            }
        }
    }
}