using System;
using System.IO;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace TaoTie
{
    public abstract class BaseEditorWindow<T> : OdinEditorWindow where T: class
    {
        protected virtual string fileName => TypeInfo<T>.Name;
        protected virtual string folderPath => "Assets/AssetsPackage";
        private bool isJson;

        public void Init(T data,string searchPath, bool isJson)
        {
            this.data = data;
            filePath = searchPath;
            this.isJson = isJson;
        }
        [Button("打开")]
        public void Open()
        {
            string searchPath = EditorUtility.OpenFilePanel($"选择{typeof(T).Name}配置文件", folderPath, "bytes");
            if (!string.IsNullOrEmpty(searchPath))
            {
                var text = File.ReadAllText(searchPath);
                try
                {
                    data = JsonHelper.FromJson<T>(text);
                    filePath = searchPath;
                    isJson = true;
                    return;
                }
                catch(Exception ex) { }
                var bytes = File.ReadAllBytes(searchPath);
                try
                {
                    data = ProtobufHelper.FromBytes<T>(bytes);
                    filePath = searchPath;
                    isJson = false;
                    return;
                }
                catch(Exception ex) { }
                data = null;
                filePath = null;
                ShowNotification(new GUIContent($"非{typeof(T).Name}文件或内容损坏"));
            }
        }

        protected virtual T CreateInstance()
        {
            return Activator.CreateInstance<T>();
        }

        [Button("新建(Json)")]
        public void CreateJson()
        {
            string searchPath = EditorUtility.SaveFilePanel($"新建{typeof(T).Name}配置文件", folderPath,fileName, "bytes");
            if (!string.IsNullOrEmpty(searchPath))
            {
                isJson = true;
                data = CreateInstance();
                filePath = searchPath;
                var jStr = JsonHelper.ToJson(data);
                File.WriteAllText(filePath, jStr);
                AssetDatabase.Refresh();
            }
        }
        [Button("新建(Nino)")]
        public void CreateNino()
        {
            string searchPath = EditorUtility.SaveFilePanel($"选择{typeof(T).Name}配置文件", folderPath,fileName, "bytes");
            if (!string.IsNullOrEmpty(searchPath))
            {
                isJson = false;
                data = CreateInstance();
                filePath = searchPath;
                var bytes = ProtobufHelper.ToBytes(data);
                File.WriteAllBytes(filePath, bytes);
                AssetDatabase.Refresh();
            }
        }

        [ShowIf("@data!=null")][ReadOnly]
        public string filePath;
        [ShowIf("@data!=null")]
        public T data;

        [Button("保存")]
        [ShowIf("@data!=null&&isJson")]
        public void SaveJson()
        {
            if (data != null && !string.IsNullOrEmpty(filePath))
            {
                var jStr = JsonHelper.ToJson(data);
                File.WriteAllText(filePath, jStr);
                AssetDatabase.Refresh();
                ShowNotification(new GUIContent("保存Json成功"));
            }
        }
        [Button("保存")]
        [ShowIf("@data!=null&&!isJson")]
        public void SaveNino()
        {
            if (data != null && !string.IsNullOrEmpty(filePath))
            {
                var bytes = ProtobufHelper.ToBytes(data);
                File.WriteAllBytes(filePath, bytes);
                AssetDatabase.Refresh();
                ShowNotification(new GUIContent("保存Nino成功"));
            }
        }
    }
}