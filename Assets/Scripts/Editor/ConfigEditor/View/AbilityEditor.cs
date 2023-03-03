using System.Collections.Generic;
using UnityEditor.Callbacks;
using UnityEditor;
using UnityEngine;

namespace TaoTie
{
    public class AbilityEditor: BaseEditorWindow<List<ConfigAbility>>
    {
        protected override string fileName => "Abilities";

        protected override string folderPath => base.folderPath + "/Unit";
        [MenuItem("Tools/配置编辑器/Ability")]
        static void OpenAbility()
        {
            EditorWindow.GetWindow<AbilityEditor>().Show();
        }
        [OnOpenAsset(1)]
        public static bool OnBaseDataOpened(int instanceID, int line)
        {
            var data = EditorUtility.InstanceIDToObject(instanceID) as TextAsset;
            var path = AssetDatabase.GetAssetPath(data);
            return InitializeData(data,path);
        }

        public static bool InitializeData(TextAsset asset,string path)
        {
            if (asset == null) return false;
            if (JsonHelper.TryFromJson<List<ConfigAbility>>(asset.text,out var ability))
            {
                var win = EditorWindow.GetWindow<AbilityEditor>();
                win.Init(ability,path,true);
                return true;
            }
            return false;
        }
    }
}