using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using Nino.Core;
using Sirenix.OdinInspector;

namespace TaoTie
{
    public class CameraEditor: BaseEditorWindow<ConfigCameras>
    {
        protected override string folderPath => base.folderPath + "/EditConfig/OthersBuildIn";

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

        [Button("打开1")]
        public void Text()
        {
            var bytes = NinoSerializer.Serialize(data);
            Log.Info(JsonHelper.ToJson(data));
            var camera = NinoDeserializer.Deserialize<ConfigCameras>(bytes);
            Log.Info(JsonHelper.ToJson(camera));
        }
    }
}