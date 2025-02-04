using System.Collections.Generic;
using UnityEditor.Callbacks;
using UnityEditor;
using UnityEngine;
#if RoslynAnalyzer
using Unity.Code.NinoGen;
#endif
namespace TaoTie
{
    public class AbilityEditor: BaseEditorWindow<List<ConfigAbility>>
    {
        protected override string fileName => "Abilities";

        protected override string folderPath => base.folderPath + "/EditConfig/Abilities";
#if RoslynAnalyzer
        protected override byte[] Serialize(List<ConfigAbility> data)
        {
            return Serializer.Serialize(data);
        }
#endif
        [MenuItem("Tools/配置编辑器/Ability")]
        static void OpenAbility()
        {
            EditorWindow.GetWindow<AbilityEditor>().Show();
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
            if (path.EndsWith(".json") && path.Contains("Abilities") && JsonHelper.TryFromJson<List<ConfigAbility>>(asset.text,out var ability))
            {
                var win = GetWindow<AbilityEditor>();
                win.Init(ability,path);
                return true;
            }
            return false;
        }
    }
}