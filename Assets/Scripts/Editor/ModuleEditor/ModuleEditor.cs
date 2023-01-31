using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TaoTIe
{

    public class ModuleEditor: EditorWindow
    {
        private const string settingAsset = "Assets/Scripts/Editor/ModuleEditor/ModuleInfo.asset";
        private string Source = "Modules";
        private string ModulePath = "Assets/Modules";
        private ModuleInfo info;
        
        Vector2 scrollPos;
        private Dictionary<string, bool> temp = new Dictionary<string, bool>();

        [MenuItem("Tools/模组管理工具")]
        public static void ShowWindow()
        {
            GetWindow(typeof (ModuleEditor));
        }

        private void OnEnable()
        {
            if (!File.Exists(settingAsset))
            {
                info = new ModuleInfo();
                AssetDatabase.CreateAsset(info, settingAsset);
            }
            else
            {
                info = AssetDatabase.LoadAssetAtPath<ModuleInfo>(settingAsset);
                Source = info.Source;
                ModulePath = info.ModulePath;
            }
            temp.Clear();
            DirectoryInfo[] content = Array.Empty<DirectoryInfo>();
            if (Directory.Exists(ModulePath))
            {
                var dir = new DirectoryInfo(ModulePath);
                content = dir.GetDirectories();
                foreach (var item in content)
                {
                    temp[item.Name] = true;
                }
            }
        }

        private void OnGUI()
        {
            Source = EditorGUILayout.TextField("仓库路径",Source);
            ModulePath = EditorGUILayout.TextField("导入路径",ModulePath);
            GUILayout.Label("");
            if (!Directory.Exists(Source))
            {
                GUILayout.Label("暂无模组");
                return;
            }

            DirectoryInfo dir = new DirectoryInfo(Source);
            var sources = dir.GetDirectories();
            if (sources.Length==0)
            {
                GUILayout.Label("暂无模组");
                return;
            }
            
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos,GUILayout.Width(position.width),GUILayout.Height(position.height-115));
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
                    bool has = Directory.Exists(ModulePath + "/" + item.Key);
                    if (item.Value && !has)
                    {
                        SafeCopyDir(Source + "/" + item.Key, ModulePath + "/" + item.Key);
                        Debug.Log("添加 "+item.Key);
                    }

                    if (!item.Value && has)
                    {
                        AssetDatabase.DeleteAsset(ModulePath + "/" + item.Key);
                        Debug.Log("移除 "+item.Key);
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
            info.Source = Source;
            info.ModulePath = ModulePath;
            EditorUtility.SetDirty(info);
            AssetDatabase.SaveAssets();
        }
        
        public static bool SafeCopyDir(string sourcePath, string savePath)
        {
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }
            #region //拷贝labs文件夹到savePath下
            try
            {
                string[] labDirs = Directory.GetDirectories(sourcePath);//目录
                string[] labFiles = Directory.GetFiles(sourcePath);//文件
                if (labFiles.Length > 0)
                {
                    for (int i = 0; i < labFiles.Length; i++)
                    {
                        //if (Path.GetExtension(labFiles[i]) == ".dll")//过滤出.dll文件
                        //{
                        File.Copy(Path.Combine(sourcePath, Path.GetFileName(labFiles[i])), Path.Combine(savePath, Path.GetFileName(labFiles[i])), true);
                        //}
                    }
                }
                if (labDirs.Length > 0)
                {
                    for (int j = 0; j < labDirs.Length; j++)
                    {
                        Directory.GetDirectories(Path.Combine(sourcePath, Path.GetFileName(labDirs[j])));

                        //递归调用
                        SafeCopyDir(Path.Combine(sourcePath, Path.GetFileName(labDirs[j])), Path.Combine(savePath, Path.GetFileName(labDirs[j])));
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            #endregion
            return true;
        }
    }
}