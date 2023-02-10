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
        readonly string folderPath = "Assets/AssetsPackage";
        [ShowIf("@ability!=null")][ReadOnly]
        public string jsonPath;
        [ShowIf("@ability!=null")]
        public List<ConfigAbility> ability;
        
        [Button("保存")]
        [ShowIf("@ability!=null")]
        public void Save()
        {
            if (ability != null && !string.IsNullOrEmpty(jsonPath))
            {
                var jStr = JsonHelper.ToJson(ability);
                File.WriteAllText(jsonPath, jStr);
                ShowNotification(new GUIContent("保存成功"));
            }
        }
        
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
                    jsonPath = searchPath;
                }
                catch(Exception ex)
                {
                    ability = null;
                    jsonPath = null;
                    ShowNotification(new GUIContent("非Ability文件或内容损坏"));
                }
            }
        }

        [Button("新建")]
        public void Create()
        {
            string searchPath = EditorUtility.SaveFilePanel("选择ability配置文件", folderPath,"ConfigAbility", "bytes");
            if (!string.IsNullOrEmpty(searchPath))
            {
                ability = new List<ConfigAbility>();
                jsonPath = searchPath;
                var jStr = JsonHelper.ToJson(ability);
                File.WriteAllText(jsonPath, jStr);
                AssetDatabase.Refresh();
            }
        }
    }
}