using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
#if RoslynAnalyzer
using Unity.Code.NinoGen;
#endif
namespace TaoTie
{
    public class InputEditor:BaseEditorWindow<ConfigInput>
    {
        protected override string folderPath => base.folderPath + "/EditConfig/OthersBuildIn";
#if RoslynAnalyzer
        protected override byte[] Serialize(ConfigInput data)
        {
            return Serializer.Serialize(data);
        }
#endif
        [MenuItem("Tools/配置编辑器/Input")]
        static void OpenSceneGroup()
        {
            EditorWindow.GetWindow<InputEditor>().Show();
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
            if (path.EndsWith(".json") && JsonHelper.TryFromJson<ConfigInput>(asset.text,out var json))
            {
                var win = GetWindow<InputEditor>();
                win.Init(json,path);
                return true;
            }
            return false;
        }

        [Button("初始化")][PropertyOrder(int.MinValue)][ShowIf("@"+nameof(data)+"!=null")]
        public void CreateDefault()
        {
            if(data == null) return;
            data.Config = new ConfigInputBinding[InputManager.Default.Length];
            for (int i = 0; i < InputManager.Default.Length; i++)
            {
                data.Config[i] = new ConfigInputBinding()
                {
                    GameBehavior = i,
                    PC = InputManager.Default[i],
                    Mobile = InputManager.Default[i],
                };
            }
        }
    }
}