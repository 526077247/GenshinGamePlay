using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace TaoTie
{
    public class SceneGroupEditor: BaseEditorWindow<ConfigSceneGroup>
    {
        protected override string folderPath => base.folderPath + "/SceneGroup";
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
            if (JsonHelper.TryFromJson<ConfigSceneGroup>(asset.text,out var json))
            {
                var win = EditorWindow.GetWindow<SceneGroupEditor>();
                win.Init(json,path,true);
                return true;
            }
            return false;
        }
    }
}