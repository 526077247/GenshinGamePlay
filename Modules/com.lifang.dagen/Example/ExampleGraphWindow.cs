using System.Collections.Generic;
using System.IO;
using DaGenGraph.Editor;
using UnityEditor;
using UnityEngine;

namespace DaGenGraph.Example
{
    public class ExampleGraphWindow: GraphWindow<ExampleGraph>
    {
        private string path;
        internal static ExampleGraphWindow instance
        {
            get
            {
                if (s_Instance != null) return s_Instance;
                var windows = Resources.FindObjectsOfTypeAll<ExampleGraphWindow>();
                s_Instance = windows.Length > 0 ? windows[0] : null;
                if (s_Instance != null) return s_Instance;
                s_Instance = CreateWindow<ExampleGraphWindow>();
                return s_Instance;
            }
        }

        private static ExampleGraphWindow s_Instance;

        [MenuItem("DaGenGraph/ExampleGraphWindow")]
        public static void GetWindow()
        {
            instance.titleContent = new GUIContent("ExampleGraphWindow");
            instance.Show();
            instance.InitGraph();
        }

        protected override void InitGraph()
        {
            path = null;
            base.InitGraph();
        }

        protected override ExampleGraph CreateGraph()
        {
            return CreateInstance<ExampleGraph>();
        }

        protected override ExampleGraph LoadGraph()
        {
            string searchPath = EditorUtility.OpenFilePanel($"新建{typeof(ExampleGraph).Name}配置文件", "Assets", "asset");
            if (!string.IsNullOrEmpty(searchPath))
            {
                searchPath = "Assets/"+searchPath.Split("/Assets/")[1];
                var obj = AssetDatabase.LoadAssetAtPath<ExampleGraph>(searchPath);
                if (obj == null) return null;
                path = searchPath;
                return obj;
            }
            return null;
        }

        protected override void SaveGraph()
        {
            if (string.IsNullOrEmpty(path))
            {
                string searchPath = EditorUtility.SaveFilePanel($"新建{typeof(ExampleGraph).Name}配置文件", "Assets",
                    typeof(ExampleGraph).Name, "asset");
                if (string.IsNullOrEmpty(searchPath)) return;
                
                path = "Assets/"+searchPath.Split("/Assets/")[1];
                AssetDatabase.CreateAsset(m_Graph,path);
            }
            else
            {
                EditorUtility.SetDirty(m_Graph);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        protected override void AddGraphMenuItems(GenericMenu menu)
        {
            var current = Event.current;
            base.AddGraphMenuItems(menu);
            menu.AddItem(new GUIContent("New/Node"), false, () =>
            {
                if (m_Graph == null)
                {
                    InitGraph();
                }
                if (string.IsNullOrEmpty(path))
                {
                    string searchPath = EditorUtility.SaveFilePanel($"新建{typeof(ExampleGraph).Name}配置文件", "Assets",
                        typeof(ExampleGraph).Name, "asset");
                    if (string.IsNullOrEmpty(searchPath)) return ;
                
                    path = "Assets/"+searchPath.Split("/Assets/")[1];
                    AssetDatabase.CreateAsset(m_Graph,path);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
                CreateNodeView(m_Graph.CreateNode<ExampleNode>(current.mousePosition));
            });
        }

        protected override void AddNodeMenuItems(GenericMenu menu, NodeBase nodeBase)
        {
            base.AddNodeMenuItems(menu, nodeBase);
            menu.AddItem(new GUIContent("AddInputPort"), false,
                () => { nodeBase.AddInputPort("InputName", EdgeMode.Multiple, true, true); });
            menu.AddItem(new GUIContent("AddOutputPort"), false,
                () => { nodeBase.AddOutputPort("OutputName", EdgeMode.Multiple, true, true); });
        }

        protected override void AddPortMenuItems(GenericMenu menu, Port port, bool isLine = false)
        {
            base.AddPortMenuItems(menu, port, isLine);
            if(!isLine)
                menu.AddItem(new GUIContent("Delete"), false, () => { RemovePort(port); });
        }
    }
}