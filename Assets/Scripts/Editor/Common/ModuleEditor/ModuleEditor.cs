using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace TaoTie
{

    public class ModuleEditor : EditorWindow
    {
        private const string packages = "Packages/manifest.json";
        private string Source = "Modules";

        Vector2 scrollPos;
        private Dictionary<string, bool> temp = new Dictionary<string, bool>();

        private Packages info;
        [MenuItem("Tools/模组管理工具")]
        public static void ShowWindow()
        {
            GetWindow(typeof(ModuleEditor));
        }

        private void OnEnable()
        {
            info = Newtonsoft.Json.JsonConvert.DeserializeObject<Packages>(File.ReadAllText(packages));
            temp.Clear();
            DirectoryInfo[] content = Array.Empty<DirectoryInfo>();
            if (Directory.Exists(Source))
            {
                var dir = new DirectoryInfo(Source);
                content = dir.GetDirectories();
                foreach (var item in content)
                {
                    temp[item.Name] = info.dependencies.ContainsKey(item.Name);
                }
            }
        }

        private void OnGUI()
        {
            Source = EditorGUILayout.TextField("仓库路径", Source);
            GUILayout.Label("");
            if (!Directory.Exists(Source))
            {
                GUILayout.Label("暂无模组");
                return;
            }

            DirectoryInfo dir = new DirectoryInfo(Source);
            var sources = dir.GetDirectories();
            if (sources.Length == 0)
            {
                GUILayout.Label("暂无模组");
                return;
            }

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(position.width),
                GUILayout.Height(position.height - 115));
            for (int i = 0; i < sources.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();
                bool old = false;
                temp.TryGetValue(sources[i].Name, out old);
                temp[sources[i].Name] = GUILayout.Toggle(old, sources[i].Name);

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
            GUILayout.Space(25);

            if (GUILayout.Button("确认"))
            {
                foreach (var item in temp)
                {
                    bool has = info.dependencies.ContainsKey(item.Key);
                    if (item.Value && !has)
                    {
                        info.dependencies.Add(item.Key, "file:../" + Source + "/" + item.Key);
                        Debug.Log("添加 " + item.Key);
                        AssetDatabase.Refresh();
                    }
                    
                    if (!item.Value && has)
                    {
                        info.dependencies.Remove(item.Key);
                        Debug.Log("移除 " + item.Key);
                    }
                }

                AssetDatabase.Refresh();
                Close();
            }
        }

        private void OnDisable()
        {
            SaveSettings();
        }

        private void SaveSettings()
        {
            File.WriteAllText(packages,Newtonsoft.Json.JsonConvert.SerializeObject(info,new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
            }));
            AssetDatabase.Refresh();
        }

        public static void Clear(string name)
        {
            Packages info =  Newtonsoft.Json.JsonConvert.DeserializeObject<Packages>(File.ReadAllText(packages));
            if (info.dependencies.ContainsKey(name))
            {
                info.dependencies.Remove(name);
                File.WriteAllText(packages,Newtonsoft.Json.JsonConvert.SerializeObject(info,new JsonSerializerSettings()
                {
                    Formatting = Formatting.Indented,
                }));
                AssetDatabase.Refresh();
            }
        }
    }
}