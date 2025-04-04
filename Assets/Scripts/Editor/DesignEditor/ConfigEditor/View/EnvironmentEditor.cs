﻿using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
#if RoslynAnalyzer
using Unity.Code.NinoGen;
#endif
namespace TaoTie
{
    public class EnvironmentEditor: BaseEditorWindow<ConfigEnvironments>
    {
        protected override string folderPath => base.folderPath + "/EditConfig";
#if RoslynAnalyzer
        protected override byte[] Serialize(ConfigEnvironments data)
        {
            return Serializer.Serialize(data);
        }
#endif
        [MenuItem("Tools/配置编辑器/Environment")]
        static void OpenAI()
        {
            EditorWindow.GetWindow<EnvironmentEditor>().Show();
        }
        [OnOpenAsset(0)]
        public static bool OnBaseDataOpened(int instanceID, int line)
        {
            var data = EditorUtility.InstanceIDToObject(instanceID) as TextAsset;
            var path = AssetDatabase.GetAssetPath(data);
            return InitializeData(data,path);
        }

        public static bool InitializeData(TextAsset asset,string path)
        {
            if (asset == null) return false;
            if (path.EndsWith(".json") && JsonHelper.TryFromJson<ConfigEnvironments>(asset.text,out var aiJson))
            {
                var win = GetWindow<EnvironmentEditor>();
                win.Init(aiJson,path);
                return true;
            }
            return false;
        }
    }
}