using System;
using System.IO;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace TaoTie
{
    public abstract class BaseEditorWindow<T> : OdinEditorWindow where T : class
    {
        protected virtual string fileName => TypeInfo<T>.TypeName;
        protected virtual string folderPath => "Assets/AssetsPackage";

        private string oldJson;

        public void Init(T data, string searchPath)
        {
            this.data = data;
            oldJson = JsonHelper.ToJson(data);
            filePath = searchPath;
        }

        protected virtual T CreateInstance()
        {
            return Activator.CreateInstance<T>();
        }
#if RoslynAnalyzer
        protected abstract byte[] Serialize(T data);
#endif
        [ShowIf("@data!=null")] [ReadOnly] public string filePath;
        [ShowIf("@data!=null")] [HideReferenceObjectPicker] public T data;

        #region Create

        [Button("打开")]
        public void Open()
        {
            string searchPath = EditorUtility.OpenFilePanel($"选择{typeof(T).Name}配置文件", folderPath, "json");
            if (!string.IsNullOrEmpty(searchPath))
            {
                var text = File.ReadAllText(searchPath);
                try
                {
                    data = JsonHelper.FromJson<T>(text);
                    filePath = searchPath;
                    return;
                }
                catch (Exception ex)
                {
                }
                data = null;
                filePath = null;
                ShowNotification(new GUIContent($"非{typeof(T).Name}文件或内容损坏"));
            }
        }

        [Button("新建")]
        public void CreateJson()
        {
            string searchPath = EditorUtility.SaveFilePanel($"新建{typeof(T).Name}配置文件", folderPath, fileName, "json");
            if (!string.IsNullOrEmpty(searchPath))
            {
                data = CreateInstance();
                filePath = searchPath;
                var jStr = JsonHelper.ToJson(data);
                oldJson = jStr;
                File.WriteAllText(filePath, jStr);
                AssetDatabase.Refresh();
            }
        }

        #endregion

        #region Save

        [Button("保存")]
        [ShowIf("@data!=null")]
        public void SaveJson()
        {
            if (data != null && !string.IsNullOrEmpty(filePath))
            {
                BeforeSaveData();
                var jStr = JsonHelper.ToJson(data);
                oldJson = jStr;
                File.WriteAllText(filePath, jStr);
#if RoslynAnalyzer
                var bytes = Serialize(data);
                File.WriteAllBytes(filePath.Replace("json","bytes"), bytes);
#endif
                AssetDatabase.Refresh();
                ShowNotification(new GUIContent("保存Json成功"));
            }
        }

        protected virtual void BeforeSaveData()
        {
            
        }
        
        [Button("另存为")]
        [ShowIf("@data!=null")]
        public void SaveNewJson()
        {
            var names = filePath.Split('/', '.');
            string name = names[names.Length - 2];
            var paths = filePath.Split(name);
            string searchPath = EditorUtility.SaveFilePanel($"新建{typeof(T).Name}配置文件", paths[0], name, "json");
            if (!string.IsNullOrEmpty(searchPath))
            {
                var jStr = JsonHelper.ToJson(data);
                oldJson = jStr;
                File.WriteAllText(searchPath, jStr);
#if RoslynAnalyzer
                var bytes = Serialize(data);
                File.WriteAllBytes(filePath.Replace("json","bytes"), bytes);
#endif
                AssetDatabase.Refresh();
                filePath = searchPath;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (data != null)
            {
                var jStr = JsonHelper.ToJson(data);
                if (oldJson != jStr)
                {
                    var res = EditorUtility.DisplayDialog("提示", "是否需要保存？", "是", "否");
                    if (res)
                    {
                        SaveJson();
                    }
                }
            }
        }
        #endregion
    }
}