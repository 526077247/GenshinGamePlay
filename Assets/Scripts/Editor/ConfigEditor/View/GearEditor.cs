using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace TaoTie
{
    public class GearEditor: BaseEditorWindow<ConfigGear>
    {
        protected override string folderPath => base.folderPath + "/Gear";
        public void Update()
        {
            OdinDropdownHelper.gear = data;
        }
        [MenuItem("Tools/配置编辑器/Gear")]
        static void OpenGear()
        {
            EditorWindow.GetWindow<GearEditor>().Show();
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
            if (JsonHelper.TryFromJson<ConfigGear>(asset.text,out var gearJson))
            {
                var win = EditorWindow.GetWindow<GearEditor>();
                win.Init(gearJson,path,true);
                return true;
            }
            return false;
        }
    }
}