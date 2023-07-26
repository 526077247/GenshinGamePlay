using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace TaoTie
{

    public class PackagesManagerEditor : EditorWindow
    {
        private const string packages = "Packages/manifest.json";
        private string Source = "Modules";

        Vector2 scrollPos;
        private Dictionary<string, bool> temp = new Dictionary<string, bool>();

        private Packages info;
        [MenuItem("Tools/Packages管理工具")]
        public static void ShowWindow()
        {
            GetWindow(typeof(PackagesManagerEditor));
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
                    string packagePath = item.FullName + "/package.json";
                    if(!File.Exists(packagePath)) return;
                    Package package = Newtonsoft.Json.JsonConvert.DeserializeObject<Package>(File.ReadAllText(packagePath));
                    temp[package.name] = info.dependencies.ContainsKey(package.name);
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
                string packagePath = sources[i].FullName + "/package.json";
                if(!File.Exists(packagePath)) return;
                Package package = Newtonsoft.Json.JsonConvert.DeserializeObject<Package>(File.ReadAllText(packagePath));
                    
                EditorGUILayout.BeginHorizontal();
                bool old = false;
                temp.TryGetValue(package.name, out old);
            
                temp[package.name] = GUILayout.Toggle(old, package.name);

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