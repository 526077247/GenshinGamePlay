using System.IO;
using DaGenGraph;
using DaGenGraph.Editor;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace TaoTie
{
    public class AIGraphWindow : GraphWindow
    {
        public string path;
        public static AIGraphWindow initance;
        
        [MenuItem("Tools/Graph编辑器/AiGraph")]
        public static void ShowAIGraph()
        {
            if (initance==null)
            {
                initance = CreateWindow<AIGraphWindow>();
                initance.titleContent = new GUIContent("AI");
            }
            initance.Show();
        }
        [OnOpenAsset(0)]
        public static bool OnBaseDataOpened(int instanceID, int line)
        {
            var data = EditorUtility.InstanceIDToObject(instanceID) as Graph;
            if (data != null)
            {
                ShowAIGraph();
                initance.Init(data);
                initance.path = AssetDatabase.GetAssetPath(data);
                return true;
            }

            return false;
        }

        protected override void ShowGraphContextMenu()
        {
            var current = Event.current;
            var menu = new GenericMenu();
            if (m_Graph.startNode == null)
            {
                menu.AddItem(new GUIContent("New/AiRootNode"), false,
                    () => { AddNode(CreateRootNode(current.mousePosition)); });
            }
            else
            {
                menu.AddItem(new GUIContent("New/AiActionNode"), false, () => { AddNode(CreateActionNode(current.mousePosition)); });
                menu.AddItem(new GUIContent("New/AiConditionNode"), false, () => { AddNode(CreateConditionNode(current.mousePosition)); });
            }
            menu.ShowAsContext();
        }
        private Node CreateRootNode(Vector2 pos, string name = "Root")
        {
            var node = CreateInstance<AIRootNode>();
            node.InitNode(m_Graph, WorldToGridPosition(pos), name);
            node.AddDefaultPorts();
            EditorUtility.SetDirty(node);
            m_Graph.startNode = node;
            return node;
        }
        private Node CreateActionNode(Vector2 pos, string name = "Action")
        {
            var node = CreateInstance<AIActionNode>();
            node.InitNode(m_Graph, WorldToGridPosition(pos), name);
            node.AddDefaultPorts();
            EditorUtility.SetDirty(node);
            return node;
        }

        private Node CreateConditionNode(Vector2 pos, string name = "Condition")
        {
            var node = CreateInstance<AIConditionNode>();
            node.InitNode(m_Graph, WorldToGridPosition(pos), name);
            node.AddDefaultPorts();
            EditorUtility.SetDirty(node);
            return node;
        }
        
        protected override void OnEnable()
        {
            base.OnEnable();
            AddButton(new GUIContent("保存"),Save);
            AddButton(new GUIContent("导出"),Export);
        }

        private void Save()
        {
            if (string.IsNullOrEmpty(path))
            {
                path = EditorUtility.SaveFilePanel($"新建AIGraph配置文件", "Assets/AssetsPackage", "AIGraph", "asset");
                if (string.IsNullOrEmpty(path))
                {
                    return;
                }

                var index = path.IndexOf("Assets/");
                path = path.Substring(index, path.Length - index);
                AssetDatabase.CreateAsset(m_Graph, path);
            }
            else
            {
                AssetDatabase.SaveAssets();
            }
        }
        private void Export()
        {
            
            
        }
    }
}