using Sirenix.OdinInspector;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using Nino.Core;
namespace TaoTie
{
    public class EntityEditor: BaseEditorWindow<ConfigActor>
    {
        protected override string folderPath => base.folderPath + "/Unit";

        [MenuItem("Tools/配置编辑器/Entity")]
        static void OpenEntity()
        {
            EditorWindow.GetWindow<EntityEditor>().Show();
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
            if (path.EndsWith(".json") && JsonHelper.TryFromJson<ConfigActor>(asset.text,out var entityJson))
            {
                var win = GetWindow<EntityEditor>();
                win.Init(entityJson,path);
                return true;
            }
            return false;
        }
    }
}