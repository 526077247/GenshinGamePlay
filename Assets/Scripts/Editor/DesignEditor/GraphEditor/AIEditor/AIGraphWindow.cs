using System.IO;
using DaGenGraph;
using DaGenGraph.Editor;
using Nino.Core;
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

        [MenuItem("Tools/Graph编辑器/AI编辑器")]
        public static void GetWindow()
        {
            instance.titleContent = new GUIContent("AI编辑器");
            instance.Show();
            instance.InitGraph();
        }
        [OnOpenAsset(0)]
        public static bool OnBaseDataOpened(int instanceID, int line)
        {
            var data = EditorUtility.InstanceIDToObject(instanceID) as TextAsset;
            var path = AssetDatabase.GetAssetPath(data);
            return InitializeData(data,path);
        }
        private static bool InitializeData(TextAsset asset,string path)
        {
            if (asset == null) return false;
            if (path.EndsWith(".json") && JsonHelper.TryFromJson<AIGraph>(asset.text,out var aiJson))
            {
                var win = instance;
                win.Show();
                win.path = path;
                win.SetGraph(aiJson);
                return true;
            }
            return false;
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            AddButton(new GUIContent("导出"),Export);
        }
        protected override string SerializeGraph()
        {
            return m_Graph != null ? JsonHelper.ToJson(m_Graph) : null;
        }

        protected override DaGenGraph.GraphBase DeserializeGraph(string json)
        {
            if (string.IsNullOrEmpty(json)) return null;
            if (JsonHelper.TryFromJson<AIGraph>(json, out var graph))
                return graph;
            return null;
        }

        protected override void OnEnterPlayMode()
        {
            if (!string.IsNullOrEmpty(path))
                SessionState.SetString(k_CachedPathKey + GetInstanceID(), path);
            base.OnEnterPlayMode();
        }

        protected override void OnExitPlayMode()
        {
            var key = k_CachedPathKey + GetInstanceID();
            var cachedPath = SessionState.GetString(key, null);
            if (!string.IsNullOrEmpty(cachedPath))
            {
                SessionState.EraseString(key);
                if (File.Exists(cachedPath))
                {
                    var jStr = File.ReadAllText(cachedPath);
                    if (JsonHelper.TryFromJson<AIGraph>(jStr, out var graph) && graph != null)
                    {
                        path = cachedPath;
                        SetGraph(graph);
                    }
                }
            }
        }

        private const string k_CachedPathKey = "DaGenGraph_CachedPath_";

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
            string searchPath = EditorUtility.OpenFilePanel($"新建{nameof(AIGraph)}配置文件",
                "Assets/AssetsPackage/GraphAssets/AITree", "json");
            if (!string.IsNullOrEmpty(searchPath))
            { 
                var jStr = File.ReadAllText(searchPath);
                var obj = JsonHelper.FromJson<AIGraph>(jStr);
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
                string searchPath = EditorUtility.SaveFilePanel($"新建{nameof(AIGraph)}配置文件",
                    "Assets/AssetsPackage/GraphAssets/AITree", nameof(AIGraph), "json");
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
                    () => { CreateNodeWithUndo(() => m_Graph.CreateNode<AIRootNode>(current.mousePosition,"Root",true)); });
            }
            else
            {
                menu.AddItem(new GUIContent("Create/AiActionNode"), false,
                    () => { CreateNodeWithUndo(() => m_Graph.CreateNode<AIActionNode>(current.mousePosition, "Action")); });
                menu.AddItem(new GUIContent("Create/AiConditionNode"), false,
                    () => { CreateNodeWithUndo(() => m_Graph.CreateNode<AIConditionNode>(current.mousePosition, "Condition")); });
            }
        }

        protected override void AddPortMenuItems(GenericMenu menu, Port port, bool isLine = false)
        {
            var current = Event.current;
            base.AddPortMenuItems(menu, port, isLine);
            if (m_Graph == null) return;
            if (port.IsOutput())
            {
                var nodeOutput = m_Graph.FindNode(port.nodeId);
                if (nodeOutput == null) return;
                if (nodeOutput is AIRootNode)
                {
                    menu.AddItem(new GUIContent($"{port.portName}Connect/AiActionNode"), false, () =>
                    {
                        PushUndoSnapshot();
                        var node = m_Graph.CreateNode<AIActionNode>(current.mousePosition, "Action");
                        CreateNodeView(node);
                        ConnectPorts(port, node.GetFirstInputPort());
                    });
                    menu.AddItem(new GUIContent($"{port.portName}Connect/AiConditionNode"), false, () =>
                    {
                        PushUndoSnapshot();
                        var node = m_Graph.CreateNode<AIConditionNode>(current.mousePosition, "Condition");
                        CreateNodeView(node);
                        ConnectPorts(port, node.GetFirstInputPort());
                    });
                }
                else if (nodeOutput is AIConditionNode)
                {
                    menu.AddItem(new GUIContent($"{port.portName}Connect/AiActionNode"), false, () =>
                    {
                        PushUndoSnapshot();
                        var node = m_Graph.CreateNode<AIActionNode>(current.mousePosition, "Action");
                        CreateNodeView(node);
                        ConnectPorts(port, node.GetFirstInputPort());
                    });
                    menu.AddItem(new GUIContent($"{port.portName}Connect/AiConditionNode"), false, () =>
                    {
                        PushUndoSnapshot();
                        var node = m_Graph.CreateNode<AIConditionNode>(current.mousePosition, "Condition");
                        CreateNodeView(node);
                        ConnectPorts(port, node.GetFirstInputPort());
                    });
                }
            }
        }

        #endregion

        #region Export

        private void Export()
        {
            if (!string.IsNullOrEmpty(path))
            {
                var name = Path.GetFileNameWithoutExtension(path);
                var exportPath = EditorUtility.SaveFilePanel($"新建AIGraph配置文件", "Assets/AssetsPackage/EditConfig/AITree/", name, "json");
                if (string.IsNullOrEmpty(exportPath))
                {
                    return;
                }
                var obj = Convert(m_Graph);
                File.WriteAllText(exportPath,JsonHelper.ToJson(obj));

                File.WriteAllBytes(exportPath.Replace("json","bytes"),NinoSerializer.Serialize(obj));

                AssetDatabase.Refresh();
                Debug.Log("导出成功");   
            }
            else
            {
                Debug.LogError("请先保存");   
            }
        
        }
        
        private ConfigAIDecisionTree Convert(AIGraph graph)
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