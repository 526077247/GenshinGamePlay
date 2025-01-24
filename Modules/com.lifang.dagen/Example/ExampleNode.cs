using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEditor;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using UnityEngine;

namespace DaGenGraph.Example
{
    [Serializable]
    public class TestClass
    {
        public string TestClassText;
    }
    [NodeViewType(typeof(ExampleNodeView))]
    public class ExampleNode: NodeBase
    {
        public string Text;
        [Min(10)]
        public int Number;
        
        [Min(10)][ValueDropdown("@Error(123)")]
        public int NumberValueDropdown;
        [DrawIgnore(Ignore.NodeView)]
        public Vector4 Vector4;

        [ReadOnly]
        public Ignore Ignore;
        
        [Range(-20,20)]
        public float Range;

        [Tooltip("测试Tooltip")]
        public Color Color;

        [ReadOnly]
        public TestClass TestClass;

        public int[] IntArray;
        public List<Rect> RectList;
        public List<TestClass> TestClasses;
        
        //详情面板选择有bug
        [JsonIgnore][OnValueChanged(nameof(SetPath))][BoxGroup("Group")]
        public GameObject GameObject;

        public void SetPath()
        {
            if (GameObject == null) Path = null;
            var path = AssetDatabase.GetAssetPath(GameObject);
            if (path.StartsWith("Assets/AssetsPackage/"))
            {
                Path = path.Replace("Assets/AssetsPackage/","");
            }
            else
            {
                Path = null;
            }
        }
        [BoxGroup("Group")][Button("ButtonTest")]
        public void Preview()
        {
            if (string.IsNullOrEmpty(Path)) return;
            if (!Path.StartsWith("Assets/AssetsPackage/")) 
                GameObject = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AssetsPackage/" +Path);
            else
                GameObject = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(Path);
            
        }
        [ReadOnly] [BoxGroup("Group")]
        public string Path;

        [JsonIgnore]
        public Sprite Sprite;

        public AnimationCurve AnimationCurve;
        [Space(15)][PropertyOrder]
        public Rect Rect;
        [NotAssets][Header("Header")][InfoBox("注意：除数不能为0")]
        public NodeBase NodeBase;

        public Dictionary<int, TestClass> TestClassDic;
        public override void AddDefaultPorts()
        {
            AddOutputPort("DefaultOutputName", EdgeMode.Multiple, true, true);
        }
    }
}