using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace TaoTie
{
    public class DecisionTreeEditor: BaseEditorWindow<DecisionNode>
    {
        protected override string fileName => "DecisionTree";
        protected override string folderPath => base.folderPath + "/Unit";
        protected override DecisionNode CreateInstance()
        {
            return new DecisionActionNode();
        }
        
              
        [MenuItem("Tools/配置编辑器/AI行为树")]
        static void OpenDecisionTree()
        {
            EditorWindow.GetWindow<DecisionTreeEditor>().Show();
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
            if (JsonHelper.TryFromJson<DecisionNode>(asset.text,out var decisionTreeJson))
            {
                var win = EditorWindow.GetWindow<DecisionTreeEditor>();
                win.Init(decisionTreeJson,path,true);
                return true;
            }
            return false;
        }
    }
}