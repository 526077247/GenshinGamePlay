using System;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace TaoTie
{
    public class AbilityEditor: OdinEditorWindow
    {
        private bool isJson;
        [Button("打开")]
        public void Open()
        {
            string searchPath = EditorUtility.OpenFilePanel("选择ability配置文件", folderPath, "bytes");
            if (!string.IsNullOrEmpty(searchPath))
            {
                var text = File.ReadAllText(searchPath);
                try
                {
                    ability = JsonHelper.FromJson<List<ConfigAbility>>(text);
                    filePath = searchPath;
                    isJson = true;
                    return;
                }
                catch(Exception ex) { }
                var bytes = File.ReadAllBytes(searchPath);
                try
                {
                    ability = ProtobufHelper.FromBytes<List<ConfigAbility>>(bytes);
                    filePath = searchPath;
                    isJson = false;
                    return;
                }
                catch(Exception ex) { }
                ability = null;
                filePath = null;
                ShowNotification(new GUIContent("非Ability文件或内容损坏"));
            }
        }

        [Button("新建(json)")]
        public void CreateJson()
        {
            string searchPath = EditorUtility.SaveFilePanel("选择ability配置文件", folderPath,"ConfigAbility", "bytes");
            if (!string.IsNullOrEmpty(searchPath))
            {
                isJson = true;
                ability = new List<ConfigAbility>();
                filePath = searchPath;
                var jStr = JsonHelper.ToJson(ability);
                File.WriteAllText(filePath, jStr);
                AssetDatabase.Refresh();
            }
        }
        [Button("新建(Nino)")]
        public void CreateNino()
        {
            string searchPath = EditorUtility.SaveFilePanel("选择ability配置文件", folderPath,"ConfigAbility", "bytes");
            if (!string.IsNullOrEmpty(searchPath))
            {
                isJson = false;
                ability = new List<ConfigAbility>();
                filePath = searchPath;
                var bytes = ProtobufHelper.ToBytes(ability);
                File.WriteAllBytes(filePath, bytes);
                AssetDatabase.Refresh();
            }
        }
        readonly string folderPath = "Assets/AssetsPackage";
        [ShowIf("@ability!=null")][ReadOnly]
        public string filePath;
        [ShowIf("@ability!=null")]
        public List<ConfigAbility> ability;
        
        [Button("保存")]
        [ShowIf("@ability!=null&&isJson")]
        public void SaveJson()
        {
            if (ability != null && !string.IsNullOrEmpty(filePath))
            {
                var jStr = JsonHelper.ToJson(ability);
                File.WriteAllText(filePath, jStr);
                ShowNotification(new GUIContent("保存Json成功"));
            }
        }
        [Button("保存")]
        [ShowIf("@ability!=null&&!isJson")]
        public void SaveNino()
        {
            if (ability != null && !string.IsNullOrEmpty(filePath))
            {
                var bytes = ProtobufHelper.ToBytes(ability);
                File.WriteAllBytes(filePath, bytes);
                ShowNotification(new GUIContent("保存Nino成功"));
            }
        }
    }
}