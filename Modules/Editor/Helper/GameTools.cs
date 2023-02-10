using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TaoTie
{
    public class GameTools
    {
        
        [MenuItem("Tools/Ability/编辑器")]
        static void OpenAbility()
        {
            EditorWindow.GetWindow<AbilityEditor>().Show();
        }
        
        [MenuItem("Tools/帮助/启动场景 #_b")]
        static void ChangeInitScene()
        {
            EditorSceneManager.OpenScene("Assets/AssetsPackage/Scenes/InitScene/Init.unity");
        }

        
        [MenuItem("Tools/帮助/创建子目录")]
        [MenuItem("Assets/工具/创建子目录")]
        public static void CreateArtSubFolder()
        {
            string[] ArtFolderNames = { "Animations", "Materials", "Models", "Textures", "Prefabs" };
            string[] UIFolderNames = { "Animations", "Atlas", "DiscreteImages","DynamicAtlas", "Prefabs" };
            string[] guids = Selection.assetGUIDs;
            for (int i = 0; i < guids.Length; i++)
            {
                string selectPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                Debug.Log(selectPath);
                if (Directory.Exists(selectPath))
                {
                    var names = ArtFolderNames;
                    selectPath.Replace("\\", "/");
                    if (selectPath.Contains("UI/") || selectPath.Contains("UIHall/") || selectPath.Contains("UIGames/"))
                    {
                        names = UIFolderNames;
                    }
                    for (int j = 0; j < names.Length; j++)
                    {
                        string folderPath = Path.Combine(selectPath, names[j]);
                        Debug.Log(folderPath);
                        Directory.CreateDirectory(folderPath);
                    }
                }
                else
                {
                    Debug.Log(selectPath + " is not a directory");
                }

            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}