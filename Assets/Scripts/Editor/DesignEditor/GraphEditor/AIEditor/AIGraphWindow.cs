using DaGenGraph;
using DaGenGraph.Editor;
using UnityEditor;
using UnityEngine;

namespace TaoTie
{
    public class AIGraphWindow : GraphWindow
    {
        [MenuItem("Tool/AiGraph")]
        public static void GetAIGraphWindow()
        {
            GetInstance<AIGraphWindow>().titleContent = new GUIContent("AI");
            GetInstance<AIGraphWindow>().Show();
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