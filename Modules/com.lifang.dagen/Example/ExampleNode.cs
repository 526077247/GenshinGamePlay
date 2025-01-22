using System;
using System.Collections.Generic;
using Newtonsoft.Json;
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
        [DrawIgnore(Ignore.NodeView)]
        public Vector4 Vector4;

        public Ignore Ignore;
        
        [Range(-20,20)]
        public float Range;

        [Tooltip("测试Tooltip")]
        public Color Color;

        public TestClass TestClass;

        public int[] IntArray;
        public List<Rect> RectList;
        public List<TestClass> TestClasses;
        //详情面板选择有bug
        [JsonIgnore]
        public GameObject GameObject;
        //详情面板选择、拖放有bug
        [JsonIgnore]
        public Sprite Sprite;

        public AnimationCurve AnimationCurve;
        [Space(15)]
        public Rect Rect;
        [NotAssets][Header("Header")]
        public NodeBase NodeBase;

        public Dictionary<int, TestClass> TestClassDic;
        public override void AddDefaultPorts()
        {
            AddOutputPort("DefaultOutputName", EdgeMode.Multiple, true, true);
        }
    }
}