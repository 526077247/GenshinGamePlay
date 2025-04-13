using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
#if RoslynAnalyzer
using Unity.Code.NinoGen;
#endif
namespace TaoTie
{
    public class SceneGroupEditor: BaseEditorWindow<ConfigSceneGroup>
    {
        protected override string folderPath => base.folderPath + "/EditConfig/SceneGroup";
#if RoslynAnalyzer
        protected override byte[] Serialize(ConfigSceneGroup data)
        {
            return Serializer.Serialize(data);
        }
#endif
        public void Update()
        {
            OdinDropdownHelper.sceneGroup = data;
        }
        [MenuItem("Tools/配置编辑器/SceneGroup")]
        static void OpenSceneGroup()
        {
            EditorWindow.GetWindow<SceneGroupEditor>().Show();
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
            if (path.EndsWith(".json") && JsonHelper.TryFromJson<ConfigSceneGroup>(asset.text,out var json))
            {
                var win = GetWindow<SceneGroupEditor>();
                OdinDropdownHelper.sceneGroup = json;
                win.Init(json,path);
                return true;
            }
            return false;
        }
    }
}