using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
#if RoslynAnalyzer
using Unity.Code.NinoGen;
#endif
namespace TaoTie
{
    public class CameraEditor: BaseEditorWindow<ConfigCameras>
    {
        protected override string folderPath => base.folderPath + "/EditConfig";
#if RoslynAnalyzer
        protected override byte[] Serialize(ConfigCameras data)
        {
            return Serializer.Serialize(data);
        }
#endif
        [MenuItem("Tools/配置编辑器/Camera")]
        static void OpenAI()
        {
            EditorWindow.GetWindow<CameraEditor>().Show();
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
            if (path.EndsWith(".json") && JsonHelper.TryFromJson<ConfigCameras>(asset.text,out var aiJson))
            {
                var win = GetWindow<CameraEditor>();
                win.Init(aiJson,path);
                return true;
            }
            return false;
        }
    }
}