using DaGenGraph;
using DaGenGraph.Editor;
using UnityEditor;
using UnityEngine;

namespace TaoTie
{
    public class AIGraphWindow : GraphWindow
    {
        public static AIGraphWindow initance;
        
        [MenuItem("Tools/AiGraph")]
        public static void ShowAIGraph()
        {
            if (initance==null)
            {
                initance = CreateWindow<AIGraphWindow>();
                initance.titleContent = new GUIContent("AI");
            }
            initance.Show();
        }

        protected override void ShowGraphContextMenu()
        {
            var current = Event.current;
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("New/AiNode"), false, () => { AddNode(CreateNode(current.mousePosition)); });
            menu.ShowAsContext();
        }

        protected override Node CreateNode(Vector2 pos, string _name = "Node")
        {
            var node = CreateInstance<AINode>();
            node.InitNode(m_Graph, WorldToGridPosition(pos), _name);
            node.AddDefaultPorts();
            EditorUtility.SetDirty(node);
            return node;
        }
    }
}