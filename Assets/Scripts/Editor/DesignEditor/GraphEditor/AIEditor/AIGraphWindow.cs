using System.IO;
using DaGenGraph;
using DaGenGraph.Editor;
using Unity.Code.NinoGen;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace TaoTie
{
    public class AIGraphWindow : GraphWindow<AIGraph>
    {
        public string path;
        
        internal static AIGraphWindow instance
        {
            get
            {
                if (s_Instance != null) return s_Instance;
                var windows = Resources.FindObjectsOfTypeAll<AIGraphWindow>();
                s_Instance = windows.Length > 0 ? windows[0] : null;
                if (s_Instance != null) return s_Instance;
                s_Instance = CreateWindow<AIGraphWindow>();
                return s_Instance;
            }
        }

        private static AIGraphWindow s_Instance;

        [MenuItem("Tools/Graph编辑器/AiGraph")]
        public static void GetWindow()
        {
            instance.titleContent = new GUIContent("AIGraphWindow");
            instance.Show();
            instance.InitGraph();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            AddButton(new GUIContent("导出"),Export);
        }
        
        protected override void InitGraph()
        {
            path = null;
            base.InitGraph();
        }

        protected override AIGraph CreateGraph()
        {
            var res = new AIGraph();
            res.CreateNode<AIRootNode>(new Vector2(position.width / 2, position.height / 2), "Root", true);
            return res;
        }

        protected override AIGraph LoadGraph()
        {
            string searchPath = EditorUtility.OpenFilePanel($"新建{typeof(AIGraph).Name}配置文件",
                "Assets/AssetsPackage/GraphAssets/AITree", "json");
            if (!string.IsNullOrEmpty(searchPath))
            { 
                var jStr = File.ReadAllText(searchPath);
                var obj = JsonHelper.FromJson<AIGraph>(jStr);
                path = searchPath;
                return obj;
            }
            return null;
        }

        protected override void SaveGraph()
        {
            if (string.IsNullOrEmpty(path))
            {
                string searchPath = EditorUtility.SaveFilePanel($"新建{typeof(AIGraph).Name}配置文件",
                    "Assets/AssetsPackage/GraphAssets/AITree", typeof(AIGraph).Name, "json");
                if (!string.IsNullOrEmpty(searchPath))
                {
                    path = searchPath;
                }
            }

            File.WriteAllText(path, JsonHelper.ToJson(m_Graph));
            AssetDatabase.Refresh();
        }

        #region Menu
        
        protected override void AddGraphMenuItems(GenericMenu menu)
        {
            var current = Event.current;
            if (m_Graph == null) InitGraph();
            if (m_Graph.startNodeId == null)
            {
                menu.AddItem(new GUIContent("Create/AiRootNode"), false,
                    () => { CreateNodeView(m_Graph.CreateNode<AIRootNode>(current.mousePosition,"Root",true)); });
            }
            else
            {
                menu.AddItem(new GUIContent("Create/AiActionNode"), false,
                    () => { CreateNodeView(m_Graph.CreateNode<AIActionNode>(current.mousePosition, "Action")); });
                menu.AddItem(new GUIContent("Create/AiConditionNode"), false,
                    () => { CreateNodeView(m_Graph.CreateNode<AIConditionNode>(current.mousePosition, "Condition")); });
            }
        }

        protected override void AddPortMenuItems(GenericMenu menu, Port port, bool isLine = false)
        {
            var current = Event.current;
            base.AddPortMenuItems(menu, port);
            if (m_Graph == null) return;
            if (port.isOutput)
            {
                var nodeOutput = m_Graph.FindNode(port.nodeId);
                if (nodeOutput == null) return;
                if (nodeOutput is AIRootNode)
                {
                    menu.AddItem(new GUIContent($"{port.portName}Connect/AiActionNode"), false, () =>
                    {
                        var node = m_Graph.CreateNode<AIActionNode>(current.mousePosition, "Action");
                        CreateNodeView(node);
                        ConnectPorts(port, node.GetFirstInputPort());
                    });
                    menu.AddItem(new GUIContent($"{port.portName}Connect/AiConditionNode"), false, () =>
                    {
                        var node = m_Graph.CreateNode<AIConditionNode>(current.mousePosition, "Condition");
                        CreateNodeView(node);
                        ConnectPorts(port, node.GetFirstInputPort());
                    });
                }
                else if (nodeOutput is AIConditionNode)
                {
                    menu.AddItem(new GUIContent($"{port.portName}Connect/AiActionNode"), false, () =>
                    {
                        var node = m_Graph.CreateNode<AIActionNode>(current.mousePosition, "Action");
                        CreateNodeView(node);
                        ConnectPorts(port, node.GetFirstInputPort());
                    });
                    menu.AddItem(new GUIContent($"{port.portName}Connect/AiConditionNode"), false, () =>
                    {
                        var node = m_Graph.CreateNode<AIConditionNode>(current.mousePosition, "Condition");
                        CreateNodeView(node);
                        ConnectPorts(port, node.GetFirstInputPort());
                    });
                }
            }
        }

        protected override void AddNodeMenuItems(GenericMenu menu, NodeBase nodeBase)
        {
            if (nodeBase.canBeDeleted)
            {
                menu.AddItem(new GUIContent("Delete"), false, () =>
                {
                    DeleteNode(nodeBase);
                });
            }
        }

        #endregion

        #region Export

        private void Export()
        {
            if (!string.IsNullOrEmpty(path))
            {
                var name = Path.GetFileNameWithoutExtension(this.path);
                var path = EditorUtility.SaveFilePanel($"新建AIGraph配置文件", "Assets/AssetsPackage/EditConfig/AITree/", name, "bytes");
                if (string.IsNullOrEmpty(path))
                {
                    return;
                }
                var obj = Convert(m_Graph);
                File.WriteAllText(path.Replace("bytes","json"),JsonHelper.ToJson(obj));
                File.WriteAllBytes(path,Serializer.Serialize(obj));
                AssetDatabase.Refresh();
                Debug.Log("导出成功");   
            }
            else
            {
                Debug.LogError("请先保存");   
            }
        
        }
        
        private ConfigAIDecisionTree Convert(GraphBase graph)
        {
            ConfigAIDecisionTree res = null;
            if (graph.GetStartNode() is AIRootNode node)
            {
                res = new ConfigAIDecisionTree();
                res.Type = node.Type;
                for (int i = 0; i < node.outputPorts.Count; i++)
                {
                    if (node.outputPorts[i].portName == "Root")
                    {
                        if (node.outputPorts[i].edges != null && node.outputPorts[i].edges.Count > 0)
                        {
                            var edge = m_Graph.GetEdge(node.outputPorts[i].edges[0]);
                            res.Node = Convert(m_Graph.FindNode(edge.inputNodeId));
                        }
                    }
                    else if (node.outputPorts[i].portName == "CombatRoot")
                    {
                        if (node.outputPorts[i].edges != null && node.outputPorts[i].edges.Count > 0)
                        {
                            var edge = m_Graph.GetEdge(node.outputPorts[i].edges[0]);
                            res.CombatNode = Convert(m_Graph.FindNode(edge.inputNodeId));
                        }
                    }
                }
              
            }
        
            return res;
        }
        
        private DecisionNode Convert(NodeBase aiNode)
        {
            if (aiNode is AIConditionNode cnode)
            {
                var res = new DecisionConditionNode();
                res.Condition = cnode.Condition;
                for (int i = 0; i < cnode.outputPorts.Count; i++)
                {
                    if (cnode.outputPorts[i].portName == "True")
                    {
                        if(cnode.outputPorts[i].edges!=null && cnode.outputPorts[i].edges.Count>0)
                        {
                            var edge = m_Graph.GetEdge(cnode.outputPorts[i].edges[0]);
                            res.True = Convert(m_Graph.FindNode(edge.inputNodeId));
                        }
                        
                    }
                    else if (cnode.outputPorts[i].portName == "False")
                    {
                        if (cnode.outputPorts[i].edges != null && cnode.outputPorts[i].edges.Count > 0)
                        {
                            var edge = m_Graph.GetEdge(cnode.outputPorts[i].edges[0]);
                            res.False = Convert(m_Graph.FindNode(edge.inputNodeId));
                        }
                    }
                }
                return res;
            }
            if (aiNode is AIActionNode anode)
            {
                var res = new DecisionActionNode();
                res.Act = anode.Data.Act;
                res.Tactic = anode.Data.Tactic;
                res.Move = anode.Data.Move;
                return res;
            }
            return null;
        }

        #endregion

    }
}