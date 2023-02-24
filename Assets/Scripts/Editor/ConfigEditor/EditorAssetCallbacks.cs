using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;

namespace TaoTie
{
    public class EditorAssetCallbacks
    {
        [OnOpenAsset(0)]
        public static bool OnBaseGraphOpened(int instanceID, int line)
        {
            var data = EditorUtility.InstanceIDToObject(instanceID) as TextAsset;
            var path = AssetDatabase.GetAssetPath(data);
            return InitializeGraph(data,path);
        }

        public static bool InitializeGraph(TextAsset asset,string path)
        {
            if (asset == null) return false;
            if (JsonHelper.TryFromJson<ConfigGear>(asset.text,out var gearJson))
            {
                var win = EditorWindow.GetWindow<GearEditor>();
                win.Init(gearJson,path,true);
                return true;
            }
            if (JsonHelper.TryFromJson<List<ConfigAbility>>(asset.text,out var gearAbility))
            {
                var win = EditorWindow.GetWindow<AbilityEditor>();
                win.Init(gearAbility,path,true);
                return true;
            }
            if (JsonHelper.TryFromJson<ConfigAIBeta>(asset.text,out var aiJson))
            {
                var win = EditorWindow.GetWindow<AIEditor>();
                win.Init(aiJson,path,true);
                return true;
            }
            if (JsonHelper.TryFromJson<ConfigEntity>(asset.text,out var entityJson))
            {
                var win = EditorWindow.GetWindow<EntityEditor>();
                win.Init(entityJson,path,true);
                return true;
            }
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