using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TaoTie
{
    public class AltasEditor
    {

        [MenuItem("Tools/美术工具/设置图片", false, 31)]
        public static void SettingPNG()
        {
            AtlasHelper.SettingPNG();
        }
        
        [MenuItem("Tools/美术工具/清理和生成图集", false, 32)]
        public static void ClearAllAtlasAndGenerate()
        {
            AtlasHelper.ClearAllAtlas();
            try
            {
                AssetDatabase.StartAssetEditing();
                AtlasHelper.GeneratingAtlas();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
        
        [MenuItem("Assets/工具/清理和生成图集",false,400)]
        static void ClearSelectionAtlasAndGenerate()
        {
            string[] guids = Selection.assetGUIDs;
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (Directory.Exists(path))
                {
                    var vs = path.Split("/");
                    if (vs[vs.Length - 2] == "AssetsPackage")
                    {
                        for (int j = 0; j < AtlasHelper.uipaths.Length; j++)
                        {
                            if (vs[vs.Length - 1] == AtlasHelper.uipaths[j])
                            {
                                //将UI目录下的Atlas 打成 图集
                                string uiPath = Path.Combine(Application.dataPath, "AssetsPackage", AtlasHelper.uipaths[j]);
                                DirectoryInfo uiDirInfo = new DirectoryInfo(uiPath);
                                foreach (DirectoryInfo dirInfo in uiDirInfo.GetDirectories())
                                {
                                    AtlasHelper.GeneratingAtlasByDir(dirInfo);
                                }
                            }
                           
                        }
                    }
                    else if (vs[vs.Length - 3] == "AssetsPackage")
                    {
                        for (int j = 0; j < AtlasHelper.uipaths.Length; j++)
                        {
                            if (vs[vs.Length - 2] == AtlasHelper.uipaths[j])
                            {
                                //将UI目录下的Atlas 打成 图集
                                DirectoryInfo uiDirInfo = new DirectoryInfo(path);
                                AtlasHelper.GeneratingAtlasByDir(uiDirInfo);
                            }
                           
                        }
                    }
                }
            }
        }

        [MenuItem("Assets/复制相对路径", false, 500)]
        static void CopyPath()
        {
            string[] guids = Selection.assetGUIDs;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < guids.Length; i++)
            {
                if (i > 0) sb.AppendLine();
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if(path.Contains("/AssetsPackage/"))
                    path = path.Split("/AssetsPackage/")[1];
                sb.Append(path);
            }

            GUIUtility.systemCopyBuffer = sb.ToString();
        }
    }
}