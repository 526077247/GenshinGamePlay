using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TaoTie
{
    public class CommonEditor
    {
        [MenuItem("Tools/帮助/启动场景 #_b")]
        static void ChangeInitScene()
        {
            EditorSceneManager.OpenScene("Assets/AssetsPackage/Scenes/InitScene/Init.unity");
        }

        
        [MenuItem("Assets/工具/创建子目录")]
        public static void CreateArtSubFolder()
        {
            string[] guids = Selection.assetGUIDs;
            for (int i = 0; i < guids.Length; i++)
            {
                string selectPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                FileHelper.CreateArtSubFolder(selectPath);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        [MenuItem("Tools/帮助/测试")]
        static void Test()
        {
            var guids = AssetDatabase.FindAssets("t:Model", new[] {"Assets/Fox"});
            for (int i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                var newAnimationClip = Object.Instantiate(clip);
                AssetDatabase.CreateAsset(newAnimationClip,"Assets/AssetsPackage/Unit/Fox/Animations/"+Path.GetFileNameWithoutExtension(path)+".anim");
                
            }
        }

    }
}