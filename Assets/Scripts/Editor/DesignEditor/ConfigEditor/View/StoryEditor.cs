using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace TaoTie
{
    public class StoryEditor:BaseEditorWindow<ConfigStory>
    {
        protected override string folderPath => base.folderPath + "/EditConfig/Story";

        [MenuItem("Tools/配置编辑器/Story")]
        static void OpenSceneGroup()
        {
            EditorWindow.GetWindow<StoryEditor>().Show();
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
            if (path.EndsWith(".json") && JsonHelper.TryFromJson<ConfigStory>(asset.text,out var json))
            {
                var win = EditorWindow.GetWindow<StoryEditor>();
                win.Init(json,path,true);
                return true;
            }
            return false;
        }
    }
}